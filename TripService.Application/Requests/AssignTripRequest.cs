//request (command input ) for assigning a trip 
//it represents user input

namespace TripService.Application.Requests;

public class AssignTripRequest
{
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
}