//message sent when vehicle status sent

namespace SharedKernel.Events;

public class VehicleStatusChangedEvent
{
    public Guid VehicleId { get; set; }
    public string Status { get; set; }
    public string VehicleType { get; set; } // i added this for cost calculation - so trip service can know the vehicle type and calculate the cost
}
