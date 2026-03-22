//message sent when a trip starts

namespace SharedKernel.Events;

public class TripStartedEvent
{
    public Guid TripId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
}