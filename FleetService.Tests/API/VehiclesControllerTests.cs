// unit tests for vehicles controller
// we verify correct http status codes (200, 404...)

//controller tests verify that endpoints return the correct http status codes

using FleetService.API.Controllers;
using FleetService.Application.DTOs;
using FleetService.Application.Requests;
using FleetService.Application.Services;
using FleetService.Application.Interfaces;
using FleetService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Messaging;
using Xunit;

namespace FleetService.Tests.API;

public class VehiclesControllerTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepoMock;
    private readonly Mock<IDriverRepository> _driverRepoMock;
    private readonly Mock<IEventPublisher> _publisherMock;
    private readonly VehiclesController _controller;
    private readonly VehicleService _service;

    public VehiclesControllerTests()
    {
        _vehicleRepoMock = new Mock<IVehicleRepository>();
        _driverRepoMock = new Mock<IDriverRepository>();
        _publisherMock = new Mock<IEventPublisher>();
        var loggerMock = new Mock<ILogger<VehicleService>>();
        var cache = new MemoryCache(new MemoryCacheOptions());

        _service = new VehicleService(
            _vehicleRepoMock.Object,
            _driverRepoMock.Object,
            _publisherMock.Object,
            loggerMock.Object,
            cache
        );

        _controller = new VehiclesController(_service);
    }

    [Fact]
    public async Task GetVehicle_WhenNotFound_ShouldReturn404()
    {
        // Arrange: repo returns null → vehicle doesn't exist
        _vehicleRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((FleetService.Domain.Entities.Vehicle?)null);

        // Act
        var result = await _controller.GetVehicle(Guid.NewGuid());

        // Assert: should be a 404 NotFound
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetVehicle_WhenFound_ShouldReturn200()
    {
        // Arrange: repo returns a real vehicle
        var vehicleId = Guid.NewGuid();
        var fakeVehicle = new FleetService.Domain.Entities.Vehicle(
            new SharedKernel.ValueObjects.PlateNumber("TEST-001"),
            "Kia",
            2021,
            VehicleType.Sedan
        );
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync(fakeVehicle);

        // Act
        var result = await _controller.GetVehicle(vehicleId);

        // Assert: should be 200 OK
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task DeleteVehicle_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _vehicleRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((FleetService.Domain.Entities.Vehicle?)null);

        // Act
        var result = await _controller.DeleteVehicle(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteVehicle_WhenFound_ShouldReturn204NoContent()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var fakeVehicle = new FleetService.Domain.Entities.Vehicle(
            new SharedKernel.ValueObjects.PlateNumber("DEL-999"),
            "Mercedes",
            2018,
            VehicleType.Truck
        );
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync(fakeVehicle);
        _vehicleRepoMock.Setup(r => r.DeleteAsync(fakeVehicle)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteVehicle(vehicleId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}
