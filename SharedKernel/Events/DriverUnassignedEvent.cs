//message sent when driver becomes unassigned 

namespace SharedKernel.Events;

public class DriverUnassignedEvent
{
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
}
