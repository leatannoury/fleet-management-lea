//request (command input ) for completing a trip 
//it represents user input

namespace TripService.Application.Requests;

public class CompleteTripRequest
{
    public Guid TripId { get; set; }
    public decimal DistanceKm { get; set; }
}