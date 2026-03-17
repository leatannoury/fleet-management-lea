//service for driver
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
    
    //get driver by id
    public async Task<DriverDto?> GetDriver(Guid id)
    {
        var driver = await _repository.GetByIdAsync(id);//send to repository

        if (driver == null)//check if null
            return null;

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
    
    //update a driver
    public async Task<DriverDto?> UpdateDriver(Guid id, UpdateDriverRequest request)
    {
        var driver = await _repository.GetByIdAsync(id);//send to repository-find the driver we want to update

        if (driver == null)//check if null
            return null;

        driver.UpdateDetails(request.FirstName, request.LastName, request.LicenseExpiry);//domain entities control their own state changes-we dont update directly from service

        await _repository.UpdateAsync(driver);//send to repository-update

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
    
    //delete a driver
    public async Task<bool> DeleteDriver(Guid id)
    {
        var driver = await _repository.GetByIdAsync(id);//send to repository-find the driver we want to delete

        if (driver == null)//check if null
            return false;

        await _repository.DeleteAsync(driver);//send to repository-delete

        return true;
    }
}