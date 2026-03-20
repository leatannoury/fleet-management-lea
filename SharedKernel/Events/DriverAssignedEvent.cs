namespace SharedKernel.Events;

public class DriverAssignedEvent
{
    public Guid DriverId { get; set; }
    public Guid VehicleId { get; set; }
}
