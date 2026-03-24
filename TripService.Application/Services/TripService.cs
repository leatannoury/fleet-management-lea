//service for trips
//implement use cases of the system
//controller calls it

using SharedKernel.Events;
using SharedKernel.Exceptions;
using TripService.Application.DTOs;
using TripService.Application.Interfaces;
using TripService.Application.Requests;
using TripService.Domain.Entities;
using SharedKernel.ValueObjects;
using TripService.Domain.Enums;
using SharedKernel.Messaging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace TripService.Application.Services;

public class TripService
{
    private readonly ITripRepository _tripRepository;
    private readonly TripTrackingService _tripTrackingService;
    private readonly IEventPublisher _publisher;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TripService> _logger;
    private readonly ITripPricingStrategy _pricingStrategy;

    public TripService(ITripRepository tripRepository, TripTrackingService tripTrackingService, IEventPublisher publisher, IMemoryCache cache, ILogger<TripService> logger, ITripPricingStrategy pricingStrategy)
    {
        _tripRepository = tripRepository;
        _tripTrackingService = tripTrackingService;
        _publisher = publisher;
        _cache = cache;
        _logger = logger;
        _pricingStrategy = pricingStrategy;
    }

   //create a trip
    public async Task<TripDto> CreateTripAsync(CreateTripRequest request)
    {
        //start location and end location are value object named location
        //so we create them from request

        //log that we are creating a trip
        _logger.LogInformation("Creating trip from {StartAddress} to {EndAddress}", request.StartAddress, request.EndAddress);

        var startLocation = new Location(request.StartLat, request.StartLng, request.StartAddress);
        var endLocation = new Location(request.EndLat, request.EndLng, request.EndAddress);

        //create a trip domain entity
        var trip = new Trip(startLocation, endLocation);

        //save in db (send it to repository and repository does it)
        await _tripRepository.AddAsync(trip);

        //log that trip was created
        _logger.LogInformation("Trip {TripId} created successfully", trip.Id);

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
        //log that we are assigning a trip
        _logger.LogInformation("Assigning Trip {TripId} to Driver {DriverId} and Vehicle {VehicleId}", tripId, request.DriverId, request.VehicleId);

        var trip = await _tripRepository.GetByIdAsync(tripId);//send to repository-get the trip by id

        if (trip == null) //check if null
            throw new DomainException("Trip not found");

        // Check local cache FIRST before assigning
        // If cache has no entry, driver is considered available (cache is populated when Fleet publishes events)
        // Cache check: Driver available? Vehicle active?
        var driverCacheStatus = _cache.Get<string>($"DriverStatus_{request.DriverId}");
        if (driverCacheStatus == "OnTrip")
            throw new DomainException("Driver is currently on another trip.");

        var vehicleCacheStatus = _cache.Get<string>($"VehicleStatus_{request.VehicleId}");
        if (vehicleCacheStatus == "Inactive" || vehicleCacheStatus == "InMaintenance" || vehicleCacheStatus == "Decommissioned")
            throw new DomainException("Vehicle is not available.");

        //assign vehicle and driver to trip (domain entity validates state transition)
        trip.Assign(request.VehicleId, request.DriverId);

        await _tripRepository.UpdateAsync(trip);//send to repository to update trip

        //log that trip assigned
        _logger.LogInformation("Trip {TripId} assigned successfully", tripId);
    }

    //start a trip
    //flow:
    //Trip Service calls StartTripAsync() → publishes "trip.started"
    //TripStartedConsumer receives it
    //Calls driverService.MarkDriverOnTrip() → sets Driver status to OnTrip in Fleet DB
    public async Task StartTripAsync(Guid tripId)
    {
        //log that we are starting a trip
        _logger.LogInformation("Starting Trip {TripId}", tripId);

        var trip = await _tripRepository.GetByIdAsync(tripId);//send to repository-get the trip by id
        
        if (trip == null)//check if null
            throw new DomainException("Trip not found");

        trip.Start();//start the trip (domain entity validates state transition)
        
        //log that trip is in progress
        _logger.LogInformation("Trip {TripId} is now InProgress", tripId);

        //trip publishes that trip started
        //Trip publishes "trip.started"
        //Fleet receives → DriverService.MarkDriverOnTrip()
        //Fleet publishes "driver.status.changed" (OnTrip)
        //Trip cache updates: DriverStatus = OnTrip
        _publisher.Publish("trip.started", new TripStartedEvent
        {
            TripId = trip.Id,
            VehicleId = (Guid)trip.VehicleId,
            DriverId = (Guid)trip.DriverId
        });

        await _tripRepository.UpdateAsync(trip);//send to repository to update trip
    }

    //complete a trip
    //flow:
//Trip Service calls Complete Trip method → saves to Trip DB → publishes "trip.completed" to RabbitMQ
// RabbitMQ holds the message in the "trip.completed" queue
// Fleet Service's TripCompletedConsumer (running in background) receives the message almost instantly
// It creates a DI scope, fetches VehicleService and DriverService
// Calls vehicleService.UpdateMileage() → updates Vehicle DB
// Calls driverService.MarkDriverAvailable() → updates Driver status in DB → publishes DriverStatusChanged event
    public async Task CompleteTripAsync(Guid tripId)
    {
        //log that we are completing a trip
        _logger.LogInformation("Completing Trip {TripId}", tripId);

        var trip = await _tripRepository.GetByIdAsync(tripId);

        if (trip == null)//check if null
            throw new DomainException("Trip not found");

        //calculate the distance of the trip based on the logs
        var calculatedDistance = await _tripTrackingService.CalculateDistanceAsync(tripId);//calculate distance method in the trip tracking service
        
        if (calculatedDistance <= 0)//validation for calculated distance
            throw new DomainException("Cannot complete a trip with 0 distance recorded.");

        var vehicleType = _cache.Get<string>($"VehicleType_{trip.VehicleId}") ?? "Sedan";

        var costMultiplier = _pricingStrategy.CalculateCost(
            vehicleType, 
            calculatedDistance, 
            trip.StartTime ?? DateTime.UtcNow, 
            DateTime.UtcNow
        );
        
        trip.Complete(calculatedDistance);//complete trip
        trip.Invoice(new Money(costMultiplier, "USD")); //automatically invoice the trip with calculated cost

        //log that trip is completed
        _logger.LogInformation("Trip {TripId} completed. Distance: {DistanceKm} km", tripId, calculatedDistance);

        //trip publishes that trip completed
        _publisher.Publish("trip.completed", new TripCompletedEvent
        {
            TripId = trip.Id,
            VehicleId = (Guid)trip.VehicleId,
            DriverId = (Guid)trip.DriverId,
            DistanceKm = trip.DistanceKm ?? 0
        });

        await _tripRepository.UpdateAsync(trip);//send to repository-update the trip
    }

    //method invoice a trip
    public async Task InvoiceTripAsync(Guid tripId, decimal amount, string currency)
    {
        //log that trip is invoicing
        _logger.LogInformation("Invoicing Trip {TripId} — Amount: {Amount} {Currency}", tripId, amount, currency);

        var trip = await _tripRepository.GetByIdAsync(tripId);//send to repository- to get trip by id

        if (trip == null)//check if null
            throw new DomainException("Trip not found");

        //money object
        var cost = new Money(amount, currency);

        //invoice method in the trip entity
        trip.Invoice(cost);
        await _tripRepository.UpdateAsync(trip);//send to repository-update trip
        
        //log that trip is invoiced
        _logger.LogInformation("Trip {TripId} invoiced successfully", tripId);
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