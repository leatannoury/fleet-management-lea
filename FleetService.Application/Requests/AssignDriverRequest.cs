//used to assign a driver to a vehicle

namespace FleetService.Application.Requests;

public class AssignDriverRequest
{
    public Guid DriverId { get; set; }
}