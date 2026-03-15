//data transfer object
//data sent to the client

using FleetService.Domain.Enums;

namespace FleetService.Application.DTOs;

public class VehicleDto
{
    public Guid Id { get; set; }
    public string PlateNumber { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public VehicleType Type { get; set; }
    public VehicleStatus Status { get; set; }
    public decimal Mileage { get; set; }
    public Guid? DriverId { get; set; }
}