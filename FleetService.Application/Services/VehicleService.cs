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

    public VehicleService(IVehicleRepository repository)//dependency injection
    {
        _repository = repository;
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
}