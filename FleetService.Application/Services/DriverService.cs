//service for driver
//implement use cases of the system
//controller calls it
//architecture: controller->application service->domain entity

using FleetService.Application.DTOs;
using FleetService.Application.Requests;
using FleetService.Domain.Entities;
using SharedKernel.ValueObjects;
using FleetService.Application.Interfaces;
using FleetService.Domain.Enums;
using SharedKernel.Messaging;
using SharedKernel.Events;
using Microsoft.Extensions.Logging;

namespace FleetService.Application.Services;

public class DriverService
{
    private readonly IDriverRepository _repository;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<DriverService> _logger;

    public DriverService(IDriverRepository repository, IEventPublisher publisher, ILogger<DriverService> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }

    
    //create driver
    public async Task<DriverDto> CreateDriver(CreateDriverRequest request)
    {
         //log action we are doing
        _logger.LogInformation("Creating driver with license {LicenseNumber}", request.LicenseNumber);

        //convert input to a domain entity
        var driver = new Driver(
            request.FirstName,
            request.LastName,
            new LicenseNumber(request.LicenseNumber),
            request.LicenseExpiry
        );

        await _repository.AddAsync(driver);

        //log that a driver is created
        _logger.LogInformation("Driver {DriverId} ({FirstName} {LastName}) created successfully", driver.Id, driver.FirstName, driver.LastName);

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
    //support filtering based on driver status 
    public async Task<List<DriverDto>> GetDrivers(DriverStatus? status = null)
    {
        var drivers = await _repository.GetAllAsync();//send to repository-get all drivers
        
        if (status.HasValue) //apply filtering
            drivers = drivers.Where(d => d.Status == status.Value).ToList();
        
        //return dto
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

        if (driver == null) //check if null
        {
            //log warning if couldnt update
            _logger.LogWarning("UpdateDriver: Driver {DriverId} not found", id);
            return null;
        }

        driver.UpdateDetails(request.FirstName, request.LastName, request.LicenseExpiry);//domain entities control their own state changes-we dont update directly from service

        await _repository.UpdateAsync(driver);//send to repository-update

        //log that driver was updated
        _logger.LogInformation("Driver {DriverId} updated successfully", driver.Id);

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
        {
            //log warning if couldn't delete
            _logger.LogWarning("DeleteDriver: Driver {DriverId} not found", id);
            return false;
        }

        await _repository.DeleteAsync(driver);//send to repository-delete driver
       //log that driver deleted
        _logger.LogInformation("Driver {DriverId} deleted", id);

        return true;
    }


    public async Task MarkDriverAvailable(Guid driverId)
    {
        var driver = await _repository.GetByIdAsync(driverId);
        if (driver == null) return;

        driver.MarkAvailable(); // logic in domain changes status to available
        await _repository.UpdateAsync(driver);
        //log that driver is available
        _logger.LogInformation("Driver {DriverId} marked as Available", driverId);

        //fleet publishes that driver status changed
        _publisher.Publish("driver.status.changed", new DriverStatusChangedEvent {
            DriverId = driver.Id,
            Status = "Available"
        });
    }

    public async Task MarkDriverOnTrip(Guid driverId)
    {
        var driver = await _repository.GetByIdAsync(driverId);
        if (driver == null) return;

        driver.MarkOnTrip(); // logic in domain changes status to on trip
        await _repository.UpdateAsync(driver);
        //log that driver is marked on trip
        _logger.LogInformation("Driver {DriverId} marked as OnTrip", driverId);

        //fleet publishes that driver status changed
        _publisher.Publish("driver.status.changed", new DriverStatusChangedEvent {
            DriverId = driver.Id,
            Status = "OnTrip"
        });
    }

}