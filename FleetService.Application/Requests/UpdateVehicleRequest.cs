//this represents the data the client send to update a vehicle

namespace FleetService.Application.Requests;

public class UpdateVehicleRequest
{
    public string Model { get; set; }

    public int Year { get; set; }

    public int Type { get; set; }
}