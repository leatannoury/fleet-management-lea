// unit tests for the vehicle entity
// we test the state machine transitions and domain rules

//domain tests check entity rules and state machine work
//no moq needed here
//just create the entity directly and call methods on it

using FleetService.Domain.Entities;
using FleetService.Domain.Enums;
using SharedKernel.Exceptions;
using SharedKernel.ValueObjects;
using Xunit;

namespace FleetService.Tests.Domain;

public class VehicleTests
{
    //method to create a basic vehicle for testing
    private Vehicle CreateVehicle()
    {
        return new Vehicle(
            new PlateNumber("ABC-1234"),
            "Toyota",
            2022,
            VehicleType.Sedan
        );
    }

    //tests for state machine

    [Fact] // [Fact] marks a method as a test
    public void Activate_FromRegistered_ShouldSetStatusToActive()
    {
        // Arrange: create a new vehicle (starts as registered)
        var vehicle = CreateVehicle();

        // Act: call Activate
        vehicle.Activate();

        // Assert: status must now be active
        Assert.Equal(VehicleStatus.Active, vehicle.Status);
    }

    [Fact]
    public void Activate_FromActive_ShouldThrowException()
    {
        // Arrange
        var vehicle = CreateVehicle();

        vehicle.Activate(); // make it active first

        // Act + Assert: activating again should throw exception
        Assert.Throws<InvalidStateTransitionException>(() => vehicle.Activate());
    }

    [Fact]
    public void RequestMaintenance_FromActive_ShouldSetStatusToInMaintenance()
    {
        // Arrange
        var vehicle = CreateVehicle();
        vehicle.Activate();

        // Act
        vehicle.RequestMaintenance();

        // Assert
        Assert.Equal(VehicleStatus.InMaintenance, vehicle.Status);
    }

    [Fact]
    public void RequestMaintenance_FromRegistered_ShouldThrowException()
    {
        // Arrange: vehicle is still Registered (not Activated)
        var vehicle = CreateVehicle();

        // Act + Assert: cannot request maintenance from Registered
        Assert.Throws<InvalidStateTransitionException>(() => vehicle.RequestMaintenance());
    }

    [Fact]
    public void CompleteMaintenance_FromInMaintenance_ShouldSetStatusToActive()
    {
        // Arrange
        var vehicle = CreateVehicle();
        vehicle.Activate();
        vehicle.RequestMaintenance();

        // Act
        vehicle.CompleteMaintenance();

        // Assert
        Assert.Equal(VehicleStatus.Active, vehicle.Status);
    }

    [Fact]
    public void Decommission_WithDriverAssigned_ShouldThrowException()
    {
        // Arrange: vehicle is active with a driver assigned
        var vehicle = CreateVehicle();
        vehicle.Activate();

        vehicle.AssignDriver(Guid.NewGuid()); // assign a driver

        // Act + Assert: cannot decommission when driver is assigned
        Assert.Throws<InvalidStateTransitionException>(() => vehicle.Decommission());
    }

    [Fact]
    public void Decommission_ActiveWithNoDriver_ShouldSucceed()
    {
        // Arrange
        var vehicle = CreateVehicle();
        vehicle.Activate();
        // no driver assigned

        // Act
        vehicle.Decommission();

        // Assert
        Assert.Equal(VehicleStatus.Decommissioned, vehicle.Status);
    }

    [Fact]
    public void Decommission_FromInMaintenance_ShouldSucceed()
    {
        // Arrange
        var vehicle = CreateVehicle();
        vehicle.Activate();
        vehicle.RequestMaintenance();

        // Act
        vehicle.Decommission();

        // Assert
        Assert.Equal(VehicleStatus.Decommissioned, vehicle.Status);
    }

    [Fact]
    public void Decommission_AlreadyDecommissioned_ShouldThrowException()
    {
        // Arrange
        var vehicle = CreateVehicle();
        vehicle.Activate();
        vehicle.Decommission();

        // Act + Assert: cannot decommission twice
        Assert.Throws<InvalidStateTransitionException>(() => vehicle.Decommission());
    }

    //tests for assignment

    [Fact]
    public void AssignDriver_WhenNoDriverAssigned_ShouldSetDriverId()
    {
        // Arrange
        var vehicle = CreateVehicle();
        var driverId = Guid.NewGuid();

        // Act
        vehicle.AssignDriver(driverId);

        // Assert
        Assert.Equal(driverId, vehicle.DriverId);
    }

    [Fact]
    public void AssignDriver_WhenAlreadyHasDriver_ShouldThrowException()
    {
        // Arrange
        var vehicle = CreateVehicle();
        vehicle.AssignDriver(Guid.NewGuid());

        // Act + Assert: cannot assign a second driver
        Assert.Throws<InvalidStateTransitionException>(() => vehicle.AssignDriver(Guid.NewGuid()));
    }

    [Fact]
    public void UnassignDriver_ShouldSetDriverIdToNull()
    {
        // Arrange
        var vehicle = CreateVehicle();
        vehicle.AssignDriver(Guid.NewGuid());

        // Act
        vehicle.UnassignDriver();

        // Assert
        Assert.Null(vehicle.DriverId);
    }

    [Fact]
    public void UpdateMileage_ShouldIncreaseMileage()
    {
        // Arrange
        var vehicle = CreateVehicle();

        // Act
        vehicle.UpdateMileage(120.5m);

        // Assert
        Assert.Equal(120.5m, vehicle.Mileage);
    }
}
