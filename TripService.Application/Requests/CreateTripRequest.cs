//request (command input ) for creating trip requests
//it represents user input

namespace TripService.Application.Requests;

public class CreateTripRequest
{
    public double StartLat { get; set; }
    public double StartLng { get; set; }
    public string StartAddress { get; set; }

    public double EndLat { get; set; }
    public double EndLng { get; set; }
    public string EndAddress { get; set; }
}