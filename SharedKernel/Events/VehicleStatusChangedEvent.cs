//message sent when vehicle status sent

namespace SharedKernel.Events;

public class VehicleStatusChangedEvent
{
    public Guid VehicleId { get; set; }
    public string Status { get; set; }
}
