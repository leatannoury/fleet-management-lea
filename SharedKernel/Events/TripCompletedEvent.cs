namespace SharedKernel.Events;

public class TripCompletedEvent
{
    public Guid TripId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public decimal DistanceKm { get; set; }
}