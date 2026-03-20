//service for trips
//implement use cases of the system
//controller calls it

using SharedKernel.Exceptions;
using TripService.Application.DTOs;
using TripService.Application.Interfaces;
using TripService.Application.Requests;
using TripService.Domain.Entities;
using SharedKernel.ValueObjects;
using TripService.Domain.Enums;

namespace TripService.Application.Services;

public class TripService
{
    private readonly ITripRepository _tripRepository;
    private readonly TripTrackingService _tripTrackingService;


    public TripService(ITripRepository tripRepository, TripTrackingService tripTrackingService)
    {
        _tripRepository = tripRepository;
        _tripTrackingService = tripTrackingService;
    }

   //create a trip
    public async Task<TripDto> CreateTripAsync(CreateTripRequest request)
    {
        //start location and end location are value object named location
        //so we create them from request
        var startLocation = new Location(request.StartLat, request.StartLng, request.StartAddress);
        var endLocation = new Location(request.EndLat, request.EndLng, request.EndAddress);

        //create a trip domain entity
        var trip = new Trip(startLocation, endLocation);

        //save in db (send it to repository and repository does it)
        await _tripRepository.AddAsync(trip);

        //return dto
        return MapToDto(trip);
    }
    
    //get all trips
    //with filtering by driver, vehicle, date range, status
    public async Task<List<TripDto>> GetAllTripsAsync(Guid? driverId = null, Guid? vehicleId = null, TripStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var trips = await _tripRepository.GetAllAsync();//send to repository-get all trips
        
        //check what filters we have from request and apply the filters
        if (driverId.HasValue)
            trips = trips.Where(t => t.DriverId == driverId.Value).ToList();
        if (vehicleId.HasValue)
            trips = trips.Where(t => t.VehicleId == vehicleId.Value).ToList();
        if (status.HasValue)
            trips = trips.Where(t => t.Status == status.Value).ToList();
        if (startDate.HasValue)
            trips = trips.Where(t => t.StartTime >= startDate.Value).ToList();
        if (endDate.HasValue)
            trips = trips.Where(t => t.EndTime <= endDate.Value).ToList();
        
        return trips.Select(MapToDto).ToList(); //return dto list
    }
    
    //get trip by id
    public async Task<TripDto?> GetTripByIdAsync(Guid id)
    {
        var trip = await _tripRepository.GetByIdAsync(id);

        if (trip == null)//check if null
            return null;

        return MapToDto(trip);//return dto
    }

   //assign a trip
    public async Task AssignTripAsync(Guid tripId, AssignTripRequest request)
    {
        var trip = await _tripRepository.GetByIdAsync(tripId);//send to repository-get the trip by id

        if (trip == null) //check if null
            throw new DomainException("Trip not found");

        //assign vehicle and driver to trip
        trip.Assign(request.VehicleId, request.DriverId);

        await _tripRepository.UpdateAsync(trip); //send to repository to update trip
    }

    //start a trip
    public async Task StartTripAsync(Guid tripId)
    {
        var trip = await _tripRepository.GetByIdAsync(tripId);//send to repository-get the trip by id

        if (trip == null)//check if null
            throw new DomainException("Trip not found");

        trip.Start(); //start trip

        await _tripRepository.UpdateAsync(trip);//send to repository to update trip
    }

    //complete a trip
    public async Task CompleteTripAsync(Guid tripId)
    {
        var trip = await _tripRepository.GetByIdAsync(tripId);//send to repository-get the trip by id

        if (trip == null) //check if null
            throw new DomainException("Trip not found");

        //calculate the distance of the trip based on the logs
        var calculatedDistance = await _tripTrackingService.CalculateDistanceAsync(tripId);//calculate distance method in the trip tracking service
        
        if (calculatedDistance <= 0)//validation for calculated distance
            throw new DomainException("Cannot complete a trip with 0 distance recorded.");
        
        trip.Complete(calculatedDistance);//complete trip
        
        await _tripRepository.UpdateAsync(trip);//send to repository-update the trip
    }

    //method invoice a trip
    public async Task InvoiceTripAsync(Guid tripId, decimal amount, string currency)
    {
        var trip = await _tripRepository.GetByIdAsync(tripId);//send to repository- to get trip by id

        if (trip == null)//check if null
            throw new DomainException("Trip not found");

        //money object
        var cost = new Money(amount, currency);

        //invoice method in the trip entity
        trip.Invoice(cost);

        await _tripRepository.UpdateAsync(trip); //send to repository-update trip
    }

    
    //method to map to dto
    private TripDto MapToDto(Trip trip)
    {
        return new TripDto
        {
            Id = trip.Id,
            VehicleId = trip.VehicleId,
            DriverId = trip.DriverId,
            StartAddress = trip.StartLocation.Address,
            EndAddress = trip.EndLocation.Address,
            Status = trip.Status.ToString(),
            StartTime = trip.StartTime,
            EndTime = trip.EndTime,
            DistanceKm = trip.DistanceKm,
            Cost = trip.Cost?.Amount
        };
    }
}