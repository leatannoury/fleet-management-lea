//vehicle entity
//represents vehicle in the fleet

using FleetService.Domain.Enums;
using SharedKernel.Common;
using SharedKernel.Exceptions;
using SharedKernel.ValueObjects;

namespace FleetService.Domain.Entities;

public class Vehicle : AggregateRoot //aggregate root ->main root entity->controls all rules inside the aggregate/all operations go through vehivle entity
{
    //properties
    public PlateNumber PlateNumber { get; private set; } //value object, instead of using string
    public string Model { get; private set; }
    public int Year { get; private set; }
    public VehicleType Type { get; private set; }//value object
    public VehicleStatus Status { get; private set; }//value object
    public decimal Mileage { get; private set; }
    public Guid? DriverId { get; private set; }
    
        
    private Vehicle() //constructor
    {
    }
    
    //constructor
    public Vehicle(PlateNumber plateNumber, string model, int year, VehicleType type)
    {
        PlateNumber = plateNumber;
        Model = model;
        Year = year;
        Type = type;

        Status = VehicleStatus.Registered; //each vehicle starts with a registered state
        Mileage = 0;//vehicle starts with 0 mileage
    }

    
    //methods to enforce transitions between vehicle states

    public void Activate() //registered → active
    {
        if (Status != VehicleStatus.Registered) //see if status is not registered
            throw new InvalidStateTransitionException(
                "Vehicle can only be activated from Registered state."); //throw exception

        Status = VehicleStatus.Active; //else set status active
    }

    public void RequestMaintenance() //active → in maintenance
    {
        if (Status != VehicleStatus.Active) //check if ststus is NOT active
            throw new InvalidStateTransitionException(
                "Maintenance can only be requested when vehicle is Active.");//throw exception

        Status = VehicleStatus.InMaintenance;//else put status as in maintenance
    }

    public void CompleteMaintenance() //in manitenance -> active
    {
        if (Status != VehicleStatus.InMaintenance) //check if the status is not in maintenance
            throw new InvalidStateTransitionException(
                "Maintenance can only be completed from InMaintenance state.");//throw exception

        Status = VehicleStatus.Active;//if status maintenance then change is to active
    }

    public void Decommission() //vehicle cant be decommisioned if the driver is assigned
    {
        if (DriverId != null) //see if the driver id is not null - it has a driver
            throw new InvalidStateTransitionException(
                "Vehicle cannot be decommissioned while a driver is assigned."); //throw exception

        if (Status == VehicleStatus.Decommissioned) //see if vehicle status is already decomminisioned
            throw new InvalidStateTransitionException(
                "Vehicle is already decommissioned."); //throw exception

        Status = VehicleStatus.Decommissioned;//else make the vehicle status as decommissioned
    }

    public void AssignDriver(Guid driverId) //can assign driver only if vehicle has no driver - prevent vehicle from having 2 drivers at once
    {
        if (DriverId != null)//first we check if it has a driver
            throw new InvalidStateTransitionException(
                "Vehicle already has a driver assigned.");//throw exception

        DriverId = driverId;//assign driver
    }

    public void UnassignDriver()//unassign driver from vehicle
    {
        DriverId = null;
    }

    public void UpdateMileage(decimal distance)//vehicle mileage is increases after each trip is completed
    {
        Mileage += distance;
    }
}