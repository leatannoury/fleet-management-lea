//driver entity
//represents a driver in the fleet

using FleetService.Domain.Enums;
using SharedKernel.Common;
using SharedKernel.Exceptions;
using SharedKernel.ValueObjects;

namespace FleetService.Domain.Entities;

public class Driver : AggregateRoot //driver is root of the driver aggregate
{
    //properties
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public LicenseNumber LicenseNumber { get; private set; }//value object
    public DateTime LicenseExpiry { get; private set; }
    public DriverStatus Status { get; private set; }
    public Guid? VehicleId { get; private set; }
    
    private Driver() //constructor
    {
    }
    
    //constructor
    public Driver(string firstName, string lastName,LicenseNumber licenseNumber,DateTime licenseExpiry)
    {
        FirstName = firstName;
        LastName = lastName;
        LicenseNumber = licenseNumber;
        LicenseExpiry = licenseExpiry;

        Status = DriverStatus.Available; //a new driver is available at first
    }
    
    
    //methods to enforce drivers rules
    //rules live in domain classes not in controllers

    public void AssignVehicle(Guid vehicleId) //assign driver to vehicle - with rule: driver can only be assigned to 1 vehicle
    {
        //driver cant be assigned a vehicle if his license is expired
        if (LicenseExpiry < DateTime.UtcNow)//check if license is expired
            throw new DriverLicenseExpiredException();//throw exception

        if (VehicleId != null)//check if the driver is assigned to a vehicle
            throw new InvalidStateTransitionException(
                "Driver already assigned to a vehicle.");//throw exception

        VehicleId = vehicleId;//assign vehicle
    }

    public void UnassignVehicle() //unassign the driver from vehcile
    {
        VehicleId = null;
    }

    public void MarkOnTrip() //when the trip starts
    {
        if (Status != DriverStatus.Available) //driver status must be available so he starts the trip. so we check it here
            throw new InvalidStateTransitionException(
                "Driver must be available to start a trip.");//throw exception

        Status = DriverStatus.OnTrip;//when the trip starts the driver status becomes OnTrip
    }

    public void MarkAvailable()//when trip finishes
    {
        Status = DriverStatus.Available;//when the trip finishes the driver's status becomes available
    }
    
    public void UpdateDetails(string firstName, string lastName, DateTime licenseExpiry)//update a driver
    {
        FirstName = firstName;
        LastName = lastName;
        LicenseExpiry = licenseExpiry;
    }
}