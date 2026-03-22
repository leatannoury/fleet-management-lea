//message sent when a trip completes

namespace SharedKernel.Events;

public class TripCompletedEvent
{
    public Guid TripId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public decimal DistanceKm { get; set; } //fleet needs this to update mileage when a trip completes
}