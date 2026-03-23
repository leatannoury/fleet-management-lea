// unit tests for the driver entity
// we test license validation and assignment rules

//domain tests check entity rules and state machine work
//no moq needed here
//just create the entity directly and call methods on it

using FleetService.Domain.Entities;
using FleetService.Domain.Enums;
using SharedKernel.Exceptions;
using SharedKernel.ValueObjects;
using Xunit;

namespace FleetService.Tests.Domain;

public class DriverTests
{
    // method to create a driver with a valid (future) license
    private Driver CreateDriver(DateTime? licenseExpiry = null)
    {
        return new Driver(
            "John",
            "Doe",
            new LicenseNumber("LIC-12345"),
            licenseExpiry ?? DateTime.UtcNow.AddYears(2) // valid license by default
        );
    }

    [Fact]
    public void AssignVehicle_WithValidLicense_ShouldSetVehicleId()
    {
        // Arrange
        var driver = CreateDriver();
        var vehicleId = Guid.NewGuid();

        // Act
        driver.AssignVehicle(vehicleId);

        // Assert
        Assert.Equal(vehicleId, driver.VehicleId);
    }

    [Fact]
    public void AssignVehicle_WithExpiredLicense_ShouldThrowException()
    {
        // Arrange: create driver with an expired license (date in the past)
        var driver = CreateDriver(licenseExpiry: DateTime.UtcNow.AddDays(-1));

        // Act + Assert
        Assert.Throws<DriverLicenseExpiredException>(() => driver.AssignVehicle(Guid.NewGuid()));
    }

    [Fact]
    public void AssignVehicle_WhenAlreadyAssigned_ShouldThrowException()
    {
        // Arrange
        var driver = CreateDriver();
        driver.AssignVehicle(Guid.NewGuid()); // first assignment

        // Act + Assert
        Assert.Throws<InvalidStateTransitionException>(() => driver.AssignVehicle(Guid.NewGuid()));
    }

    [Fact]
    public void UnassignVehicle_ShouldSetVehicleIdToNull()
    {
        // Arrange
        var driver = CreateDriver();
        driver.AssignVehicle(Guid.NewGuid());

        // Act
        driver.UnassignVehicle();

        // Assert
        Assert.Null(driver.VehicleId);
    }

    [Fact]
    public void MarkOnTrip_WhenAvailable_ShouldSetStatusToOnTrip()
    {
        // Arrange
        var driver = CreateDriver();
        // new driver starts as Available

        // Act
        driver.MarkOnTrip();

        // Assert
        Assert.Equal(DriverStatus.OnTrip, driver.Status);
    }

    [Fact]
    public void MarkOnTrip_WhenAlreadyOnTrip_ShouldThrowException()
    {
        // Arrange
        var driver = CreateDriver();
        driver.MarkOnTrip();

        // Act + Assert: cannot start another trip while already on one
        Assert.Throws<InvalidStateTransitionException>(() => driver.MarkOnTrip());
    }

    [Fact]
    public void MarkAvailable_ShouldSetStatusToAvailable()
    {
        // Arrange
        var driver = CreateDriver();
        driver.MarkOnTrip();

        // Act
        driver.MarkAvailable();

        // Assert
        Assert.Equal(DriverStatus.Available, driver.Status);
    }

    [Fact]
    public void NewDriver_ShouldStartAsAvailable()
    {
        // Arrange + Act
        var driver = CreateDriver();

        // Assert: new drivers always start Available
        Assert.Equal(DriverStatus.Available, driver.Status);
    }
}
