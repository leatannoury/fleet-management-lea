//service for trip tracking
//for logs and distance
//handle tracking

using SharedKernel.Exceptions;
using TripService.Application.Interfaces;
using TripService.Application.Requests;
using TripService.Domain.Entities;
using SharedKernel.ValueObjects;
using TripService.Domain.Enums;

public class TripTrackingService
{
    private readonly ITripRepository _tripRepository;
    private readonly ITripLogRepository _logRepository;

    public TripTrackingService(ITripRepository tripRepository, ITripLogRepository logRepository) //dependenncy injection
    {
        _tripRepository = tripRepository;
        _logRepository = logRepository;
    }

    //add location log
    public async Task AddLocationLogAsync(AddLocationLogRequest request)
    {
        var trip = await _tripRepository.GetByIdAsync(request.TripId);//send to repository-get the trip by id

        if (trip == null) //check if null
            throw new DomainException("Trip not found");
        
        if (trip.Status != TripStatus.InProgress)
            throw new DomainException("Trip must be in progress to add logs.");

        //create value object for location from request
        var location = new Location(request.Latitude, request.Longitude, request.Address);

        //create the log entity
        var log = new TripLog(request.TripId, location, request.Speed);

        await _logRepository.AddAsync(log); //send to repository to add trip log
    }

   //calculate distance
    public async Task<decimal> CalculateDistanceAsync(Guid tripId)
    {
        var trip = await _tripRepository.GetByIdAsync(tripId);//fetch trip

        if (trip == null) //check if null
            throw new DomainException("Trip not found");
        
        var logs = await _logRepository.GetByTripIdAsync(tripId); //send to repository-get the logs for the trip
        
        if (logs == null || logs.Count < 2) //we need at least 2 pts to calculate distance
            return 0;

        logs = logs.OrderBy(l => l.Timestamp).ToList();//to order the logs by time
        
        
        decimal totalDistance = 0m; //initialize total distance variabl

        //distance calculation
        //we have pts like A(lat, long) B(lat,long)... each one of them is a trip log
        for (int i = 1; i < logs.Count; i++) //loop through trip logs
        {
            //we start comparing points previous and current ex: prev A curr B, prev B curr C
            
            //get previous and current points
            var prev = logs[i - 1].Location;
            var curr = logs[i].Location;

            //calculate the difference between 2 points
            var distance = (decimal)Math.Sqrt(
                Math.Pow(curr.Latitude - prev.Latitude, 2) +
                Math.Pow(curr.Longitude - prev.Longitude, 2)
            ); //pythagore formula for distance: (lat2 - lat1)^2 + (long2 - long1)^2

            totalDistance += distance; //add all distances (tot=AB+BC+CD....)
        }
        


        //update trip with the calculated distance
        trip.SetDistance(totalDistance);

        await _tripRepository.UpdateAsync(trip);//send to repository to update trip

        return totalDistance;
    }
}