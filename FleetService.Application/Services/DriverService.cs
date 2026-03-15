//service
//implement use cases of the system
//controller calls it
//architecture: controller->application service->domain entity

using FleetService.Application.DTOs;
using FleetService.Application.Requests;
using FleetService.Domain.Entities;
using SharedKernel.ValueObjects;
using FleetService.Application.Interfaces;

namespace FleetService.Application.Services;

public class DriverService
{


    private readonly IDriverRepository _repository;

    public DriverService(IDriverRepository repository)//dependency injection
    {
        _repository = repository;
    }

    
    //create driver
    public async Task<DriverDto> CreateDriver(CreateDriverRequest request)
    {
        //controller receives json, it becomes CreateVehicleRequest, the service convert it to a domain entity, and return a dto

        //convert input to a domain entity
        var driver = new Driver(
            request.FirstName,
            request.LastName,
            new LicenseNumber(request.LicenseNumber), //create value object 
            request.LicenseExpiry
        );

        await _repository.AddAsync(driver); //save entity to repository

        //convert to dto and return response
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

    //get all drivers
    public async Task<List<DriverDto>> GetDrivers()
    {
        var drivers=await _repository.GetAllAsync();
        
        //convert each entity to a dto 
        return drivers.Select(d => new DriverDto
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