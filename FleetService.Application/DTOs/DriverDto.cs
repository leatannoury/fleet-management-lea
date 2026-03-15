//data transfer object
//data sent to the client

using FleetService.Domain.Enums;

namespace FleetService.Application.DTOs;

public class DriverDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string LicenseNumber { get; set; }
    public DateTime LicenseExpiry { get; set; }
    public DriverStatus Status { get; set; }
    public Guid? VehicleId { get; set; }
}