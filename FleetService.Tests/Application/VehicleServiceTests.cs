// unit tests for vehicle service (application layer)
// we use moq to fake the repositories — no real database needed

//service tests check that application service layer calls the right things

using FleetService.Application.Interfaces;
using FleetService.Application.Requests;
using FleetService.Application.Services;
using FleetService.Domain.Entities;
using FleetService.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Messaging;
using SharedKernel.ValueObjects;
using Xunit;

namespace FleetService.Tests.Application;

public class VehicleServiceTests
{
    // mock objects — fake versions of the interfaces
    private readonly Mock<IVehicleRepository> _vehicleRepoMock;
    private readonly Mock<IDriverRepository> _driverRepoMock;
    private readonly Mock<IEventPublisher> _publisherMock;
    private readonly Mock<ILogger<VehicleService>> _loggerMock;
    private readonly IMemoryCache _cache;
    private readonly VehicleService _service;

    public VehicleServiceTests()
    {
        // create all mocks
        _vehicleRepoMock = new Mock<IVehicleRepository>();
        _driverRepoMock = new Mock<IDriverRepository>();
        _publisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<VehicleService>>();

        // create a real memory cache (it not need mocking)
        _cache = new MemoryCache(new MemoryCacheOptions());

        // create the real vehicle service, inject all mocks
        _service = new VehicleService(
            _vehicleRepoMock.Object,
            _driverRepoMock.Object,
            _publisherMock.Object,
            _loggerMock.Object,
            _cache
        );
    }

    [Fact]// [Fact] marks a method as a test
    public async Task CreateVehicle_ShouldCallAddAsync_AndReturnDto()
    {
        // Arrange: set up the request
        var request = new CreateVehicleRequest
        {
            PlateNumber = "XYZ-9999",
            Model = "Honda",
            Year = 2023,
            Type = VehicleType.Sedan
        };

        // we tell moq when AddAsync is called, do nothing (just return)
        _vehicleRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Vehicle>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateVehicle(request);

        // Assert: the result should match our input
        Assert.Equal("XYZ-9999", result.PlateNumber);
        Assert.Equal("Honda", result.Model);
        Assert.Equal(2023, result.Year);

        // verify that AddAsync was actually called once
        _vehicleRepoMock.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Once);
    }

    [Fact]
    public async Task GetVehicle_WhenExists_ShouldReturnDto()
    {
        // Arrange: create a fake vehicle
        var vehicleId = Guid.NewGuid();
        var fakeVehicle = new Vehicle(
            new PlateNumber("ABC-0001"),
            "BMW",
            2020,
            VehicleType.SUV
        );

        // tell Moq: when GetByIdAsync is called with vehicleId, return fakeVehicle
        _vehicleRepoMock
            .Setup(r => r.GetByIdAsync(vehicleId))
            .ReturnsAsync(fakeVehicle);

        // Act
        var result = await _service.GetVehicle(vehicleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABC-0001", result!.PlateNumber);
    }

    [Fact]
    public async Task GetVehicle_WhenNotExists_ShouldReturnNull()
    {
        // Arrange: tell Moq to return null (vehicle not found)
        _vehicleRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        var result = await _service.GetVehicle(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteVehicle_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange
        _vehicleRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        var result = await _service.DeleteVehicle(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteVehicle_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var fakeVehicle = new Vehicle(new PlateNumber("DEL-001"), "Ford", 2019, VehicleType.Van);

        _vehicleRepoMock.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync(fakeVehicle);
        _vehicleRepoMock.Setup(r => r.DeleteAsync(fakeVehicle)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteVehicle(vehicleId);

        // Assert
        Assert.True(result);
        _vehicleRepoMock.Verify(r => r.DeleteAsync(fakeVehicle), Times.Once);
    }
}
