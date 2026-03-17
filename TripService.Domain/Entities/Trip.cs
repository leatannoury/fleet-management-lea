//trip entity

using SharedKernel.Common;
using SharedKernel.ValueObjects;
using TripService.Domain.Enums;

namespace TripService.Domain.Entities;

public class Trip : AggregateRoot //trip is the root of the trip aggregate
{
    //properties of trip
    public Guid Id { get; private set; }

    public Guid? VehicleId { get; private set; }

    public Guid? DriverId { get; private set; }

    public Location StartLocation { get; private set; } //value object

    public Location EndLocation { get; private set; }

    public TripStatus Status { get; private set; } //enum

    public DateTime? StartTime { get; private set; }

    public DateTime? EndTime { get; private set; }

    public decimal? DistanceKm { get; private set; }

    public Money? Cost { get; private set; } //value object
    

    private Trip() { } // constructor for ef core

    public Trip(Location startLocation, Location endLocation)//constructor
    {
        Id = Guid.NewGuid();
        StartLocation = startLocation;
        EndLocation = endLocation;
        Status = TripStatus.Requested;
    }
}