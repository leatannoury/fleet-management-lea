namespace SharedKernel.Events;

public class DriverStatusChangedEvent
{
    public Guid DriverId { get; set; }
    public string Status { get; set; }
}