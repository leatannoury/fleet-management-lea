// unit tests for the trip entity
// we test the trip lifecycle: requested → assigned → in progress → completed → invoiced

//domain tests check entity rules and state machine work
//no moq needed here
//just create the entity directly and call methods on it

using SharedKernel.Exceptions;
using SharedKernel.ValueObjects;
using TripService.Domain.Entities;
using TripService.Domain.Enums;
using Xunit;

namespace TripService.Tests.Domain;

public class TripTests
{
    //method to create a basic trip
    private Trip CreateTrip()
    {
        var start = new Location(33.88, 35.49, "Beirut");
        var end = new Location(33.89, 35.50, "Zahle");
        return new Trip(start, end);
    }

    [Fact]// [Fact] marks a method as a test
    public void NewTrip_ShouldHaveStatusRequested()
    {
        // Arrange + Act
        var trip = CreateTrip();

        // Assert
        Assert.Equal(TripStatus.Requested, trip.Status);
    }

    [Fact]
    public void Assign_FromRequested_ShouldSetStatusToAssigned()
    {
        // Arrange
        var trip = CreateTrip();

        // Act
        trip.Assign(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(TripStatus.Assigned, trip.Status);
    }

    [Fact]
    public void Assign_WithEmptyVehicleId_ShouldThrowException()
    {
        // Arrange
        var trip = CreateTrip();

        // Act + Assert: empty GUIDs are invalid
        Assert.Throws<DomainException>(() => trip.Assign(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Assign_WhenAlreadyAssigned_ShouldThrowException()
    {
        // Arrange
        var trip = CreateTrip();
        trip.Assign(Guid.NewGuid(), Guid.NewGuid());

        // Act + Assert: cannot assign again once already assigned
        Assert.Throws<InvalidStateTransitionException>(() => trip.Assign(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Start_FromAssigned_ShouldSetStatusToInProgress()
    {
        // Arrange
        var trip = CreateTrip();
        trip.Assign(Guid.NewGuid(), Guid.NewGuid());

        // Act
        trip.Start();

        // Assert
        Assert.Equal(TripStatus.InProgress, trip.Status);
        Assert.NotNull(trip.StartTime); // start time should be set
    }

    [Fact]
    public void Start_FromRequested_ShouldThrowException()
    {
        // Arrange: trip not yet assigned
        var trip = CreateTrip();

        // Act + Assert: must be Assigned before Starting
        Assert.Throws<InvalidStateTransitionException>(() => trip.Start());
    }

    [Fact]
    public void Complete_FromInProgress_ShouldSetStatusToCompleted()
    {
        // Arrange
        var trip = CreateTrip();
        trip.Assign(Guid.NewGuid(), Guid.NewGuid());
        trip.Start();

        // Act
        trip.Complete(50.5m);

        // Assert
        Assert.Equal(TripStatus.Completed, trip.Status);
        Assert.Equal(50.5m, trip.DistanceKm);
        Assert.NotNull(trip.EndTime);
    }

    [Fact]
    public void Complete_WithZeroDistance_ShouldThrowException()
    {
        // Arrange
        var trip = CreateTrip();
        trip.Assign(Guid.NewGuid(), Guid.NewGuid());
        trip.Start();

        // Act + Assert: distance must be > 0
        Assert.Throws<DomainException>(() => trip.Complete(0));
    }

    [Fact]
    public void Complete_FromAssigned_ShouldThrowException()
    {
        // Arrange: trip assigned but not started
        var trip = CreateTrip();
        trip.Assign(Guid.NewGuid(), Guid.NewGuid());

        // Act + Assert
        Assert.Throws<InvalidStateTransitionException>(() => trip.Complete(10m));
    }

    [Fact]
    public void Invoice_FromCompleted_ShouldSetStatusToInvoiced()
    {
        // Arrange
        var trip = CreateTrip();
        trip.Assign(Guid.NewGuid(), Guid.NewGuid());
        trip.Start();
        trip.Complete(50m);

        // Act
        trip.Invoice(new Money(100m, "USD"));

        // Assert
        Assert.Equal(TripStatus.Invoiced, trip.Status);
        Assert.NotNull(trip.Cost);
    }

    [Fact]
    public void Invoice_WithNullCost_ShouldThrowException()
    {
        // Arrange
        var trip = CreateTrip();
        trip.Assign(Guid.NewGuid(), Guid.NewGuid());
        trip.Start();
        trip.Complete(50m);

        // Act + Assert
        Assert.Throws<DomainException>(() => trip.Invoice(null!));
    }

    [Fact]
    public void Invoice_FromInProgress_ShouldThrowException()
    {
        // Arrange
        var trip = CreateTrip();
        trip.Assign(Guid.NewGuid(), Guid.NewGuid());
        trip.Start();

        // Act + Assert: must be Completed before invoicing
        Assert.Throws<InvalidStateTransitionException>(() => trip.Invoice(new Money(100m, "USD")));
    }
}
