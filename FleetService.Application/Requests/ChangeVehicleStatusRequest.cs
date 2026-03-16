//to change the vehicle status

using FleetService.Domain.Enums;

namespace FleetService.Application.Requests;

public class ChangeVehicleStatusRequest
{
    public VehicleStatus Status { get; set; }
}