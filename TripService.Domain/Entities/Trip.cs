//trip entity

using SharedKernel.Common;
using SharedKernel.Exceptions;
using SharedKernel.ValueObjects;
using TripService.Domain.Enums;

namespace TripService.Domain.Entities;

public class Trip : AggregateRoot //trip is the root of the trip aggregate
{
    //properties of trip
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
        StartLocation = startLocation;
        EndLocation = endLocation;
        Status = TripStatus.Requested;
    }
    
    //assign vehicle and driver to a trip
    //driver must be available
    //vehicle must be active (those in application layer not here)
    public void Assign(Guid vehicleId, Guid driverId)
    {
        if (vehicleId == Guid.Empty || driverId == Guid.Empty)//validation
            throw new DomainException("Invalid vehicle or driver.");
        
        if (Status != TripStatus.Requested) //check that trip status is requested
            throw new InvalidStateTransitionException("Trip must be Requested to assign.");

        //assign
        VehicleId = vehicleId;
        DriverId = driverId;

        Status = TripStatus.Assigned; //change trip status to assigned
    }
    
    //method to start trip
    public void Start()
    {
        if (Status != TripStatus.Assigned) //check that the trip status is assigned to be able to start
            throw new InvalidStateTransitionException("Trip must be Assigned to start.");

        Status = TripStatus.InProgress; //change the trip sttaus to in progress
        StartTime = DateTime.UtcNow; //save the time now as start time of the trip
    }
    
    //when trip finishes
    public void Complete(decimal distanceKm)
    {
        if (distanceKm <= 0)//input validation
            throw new DomainException("Distance must be greater than 0");
        
        if (Status != TripStatus.InProgress)//check that the trip status is in progress to be able to make it complete
            throw new InvalidStateTransitionException("Trip must be InProgress to complete.");

        DistanceKm = distanceKm; //update the distance
        EndTime = DateTime.UtcNow; //save the time now as end time of thr trip

        Status = TripStatus.Completed; //change the status of the trip to completed
    }
    
    //invoice after the trip completes
    public void Invoice(Money cost)
    {
        if (cost == null)//validation
            throw new DomainException("Cost cannot be null.");
        
        if (cost.Amount <= 0)//input validation
            throw new DomainException("Cost must be positive");
        
        if (Status != TripStatus.Completed) //check that the trip status is completed to be able to invoice
            throw new InvalidStateTransitionException("Trip must be Completed to invoice.");

        Cost = cost; //save the cost of the trip
        Status = TripStatus.Invoiced; //update the status of the trip to invoiced
    }
    
    //to update distance
    public void SetDistance(decimal distanceKm)
    {
        DistanceKm = distanceKm;
    }
}