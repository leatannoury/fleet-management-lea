//dto for trip
//what api returns
//so we don't return domain entities directly

namespace TripService.Application.DTOs;

public class TripDto
{
    public Guid Id { get; set; }

    public Guid? VehicleId { get; set; }

    public Guid? DriverId { get; set; }

    public string StartAddress { get; set; }

    public string EndAddress { get; set; }

    public string Status { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public decimal? DistanceKm { get; set; }

    public decimal? Cost { get; set; }
}