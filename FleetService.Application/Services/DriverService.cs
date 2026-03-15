//service
//implement use cases of the system
//controller calls it
//architecture: controller->application service->domain entity

using FleetService.Application.DTOs;
using FleetService.Application.Requests;
using FleetService.Domain.Entities;
using SharedKernel.ValueObjects;

namespace FleetService.Application.Services;

public class DriverService
{
    private readonly List<Driver> _drivers = new();//temporary now later postgre

    public DriverDto CreateDriver(CreateDriverRequest request)
    {
        //controller receives json, it becomes CreateVehicleRequest, the service convert it to a domain entity, and return a dto

        var driver = new Driver(
            request.FirstName,
            request.LastName,
            new LicenseNumber(request.LicenseNumber),
            request.LicenseExpiry
        );

        _drivers.Add(driver);

        return new DriverDto
        {
            Id = driver.Id,
            FirstName = driver.FirstName,
            LastName = driver.LastName,
            LicenseNumber = driver.LicenseNumber.Value,
            LicenseExpiry = driver.LicenseExpiry,
            Status = driver.Status,
            VehicleId = driver.VehicleId
        };
    }

    public List<DriverDto> GetDrivers()
    {
        return _drivers.Select(d => new DriverDto
        {
            Id = d.Id,
            FirstName = d.FirstName,
            LastName = d.LastName,
            LicenseNumber = d.LicenseNumber.Value,
            LicenseExpiry = d.LicenseExpiry,
            Status = d.Status,
            VehicleId = d.VehicleId
        }).ToList();
    }
}