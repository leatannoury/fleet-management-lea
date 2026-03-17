//request (command input ) for starting a trip 
//it represents user input

namespace TripService.Application.Requests;

public class StartTripRequest
{
    public Guid TripId { get; set; }
}