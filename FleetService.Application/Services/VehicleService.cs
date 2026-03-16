//service
//implement use cases of the system
//controller calls it
//architecture: controller->application service->domain entity

using FleetService.Application.DTOs;
using FleetService.Application.Requests;
using FleetService.Domain.Entities;
using FleetService.Domain.Enums;
using FleetService.Application.Interfaces;
using SharedKernel.ValueObjects;


namespace FleetService.Application.Services;

public class VehicleService
{
    private readonly IVehicleRepository _repository;
    private readonly IDriverRepository _driverRepository;

    public VehicleService(IVehicleRepository repository, IDriverRepository driverRepository)//constructor-dependency injection
    {
        _repository = repository;
        _driverRepository = driverRepository;
    }

    //create vehicle
    public async Task<VehicleDto> CreateVehicle(CreateVehicleRequest request)
    {
        //controller receives json, it becomes CreateVehicleRequest, the service convert it to a domain entity, and return a dto
        
        //convert input to domain entity
        var vehicle = new Vehicle(
            new PlateNumber(request.PlateNumber),
            request.Model,
            request.Year,
            request.Type
        );

        await _repository.AddAsync(vehicle);//send to repository

        return new VehicleDto
        {
            Id = vehicle.Id,
            PlateNumber = vehicle.PlateNumber.Value,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Type = vehicle.Type,
            Status = vehicle.Status,
            Mileage = vehicle.Mileage,
            DriverId = vehicle.DriverId
        };
    }
    
    
    //get all vehicles
    public async Task<List<VehicleDto>> GetVehicles()
    {
        var vehicles = await _repository.GetAllAsync();//send to repository

        //convert to dto
        return vehicles.Select(v => new VehicleDto
        {
            Id = v.Id,
            PlateNumber = v.PlateNumber.Value,
            Model = v.Model,
            Year = v.Year,
            Type = v.Type,
            Status = v.Status,
            Mileage = v.Mileage,
            DriverId = v.DriverId
        }).ToList();
    }
    
    //get vehicle by id
    public async Task<VehicleDto?> GetVehicle(Guid id)
    {
        var vehicle = await _repository.GetByIdAsync(id);//send to repository

        if (vehicle == null)//check if null
            return null;

        return new VehicleDto
        {
            Id = vehicle.Id,
            PlateNumber = vehicle.PlateNumber.Value,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Type = vehicle.Type,
            Status = vehicle.Status,
            Mileage = vehicle.Mileage,
            DriverId = vehicle.DriverId
        };
    }
    
    //update a vehicle
    public async Task<VehicleDto?> UpdateVehicle(Guid id, UpdateVehicleRequest request)
    {
        var vehicle = await _repository.GetByIdAsync(id);//send to repository - find the vehicle by id

        if (vehicle == null)//check if null
            return null;

        vehicle.UpdateDetails(request.Model, request.Year, (VehicleType)request.Type);//domain entities control their own state changes-we dont update directly from service

        await _repository.UpdateAsync(vehicle);//send to repository - update vehicle

        return new VehicleDto
        {
            Id = vehicle.Id,
            PlateNumber = vehicle.PlateNumber.Value,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Type = vehicle.Type,
            Status = vehicle.Status,
            Mileage = vehicle.Mileage,
            DriverId = vehicle.DriverId
        };
    }
    
    //delete a vehicle
    public async Task<bool> DeleteVehicle(Guid id)
    {
        var vehicle = await _repository.GetByIdAsync(id);//send to repository-find the vehicle we want to delete

        if (vehicle == null)//check if null
            return false;

        await _repository.DeleteAsync(vehicle);//send to repository-delete vehicle

        return true;
    }
    
    //assign driver to a vehicle
    public async Task<bool> AssignDriver(Guid vehicleId, Guid driverId)
    {
        var vehicle = await _repository.GetByIdAsync(vehicleId);//send to repository-get the vehicle by id
        if (vehicle == null)//check if null
            return false;

        var driver = await _driverRepository.GetByIdAsync(driverId);//send to repository-get driver by id
        if (driver == null)//check if null
            return false;

        //domain logic
        vehicle.AssignDriver(driver.Id); //assign driver to vehicle
        driver.AssignVehicle(vehicle.Id);//assign vehicle to driver

        await _repository.UpdateAsync(vehicle);//send to repository-update vehivle details
        await _driverRepository.UpdateAsync(driver);//send to repository-update driver details

        return true;
    }
    
    //unassign driver from vehicle
    public async Task<bool> UnassignDriver(Guid vehicleId)
    {
        var vehicle = await _repository.GetByIdAsync(vehicleId);//send to repository-get the vehicle by id 

        if (vehicle == null)//check if null
            return false;

        if (vehicle.DriverId == null)//check if already doesnt have a driver
            return false;

        var driver = await _driverRepository.GetByIdAsync(vehicle.DriverId.Value);//send to repository-get driver by id

        vehicle.UnassignDriver();//unassign driver from vehicle

        if (driver != null)//check if driver is not null
            driver.UnassignVehicle();//unassign vehicle from driver

        await _repository.UpdateAsync(vehicle);//send to repository-update vehicle

        if (driver != null)
            await _driverRepository.UpdateAsync(driver);//send to repository-update driver

        return true;
    }
}