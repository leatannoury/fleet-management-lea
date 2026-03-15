//request model
//represent data coming FROM the client (post)

using FleetService.Domain.Enums;

namespace FleetService.Application.Requests;

public class CreateVehicleRequest
{
    public string PlateNumber { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public VehicleType Type { get; set; }
}