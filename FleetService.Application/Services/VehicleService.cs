//service
//implement use cases of the system
//controller calls it
//architecture: controller->application service->domain entity

using FleetService.Application.DTOs;
using FleetService.Application.Requests;
using FleetService.Domain.Entities;
using FleetService.Domain.Enums;
using FleetService.Application.Interfaces;
using SharedKernel.Exceptions;
using SharedKernel.ValueObjects;
using SharedKernel.Messaging;
using SharedKernel.Events;


namespace FleetService.Application.Services;

public class VehicleService
{
    private readonly IVehicleRepository _repository;
    private readonly IDriverRepository _driverRepository;
    private readonly IEventPublisher _publisher;

    public VehicleService(IVehicleRepository repository, IDriverRepository driverRepository, IEventPublisher publisher)//constructor-dependency injection
    {
        _repository = repository;
        _driverRepository = driverRepository;
        _publisher = publisher;
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
    //with accept filters vehicle status and vehicle type
    public async Task<List<VehicleDto>> GetVehicles(VehicleStatus? status = null, VehicleType? type = null)
    {
        var vehicles = await _repository.GetAllAsync(); //send to repository-get all vehicles

        //filter result
        if (status.HasValue)
            vehicles = vehicles.Where(v => v.Status == status.Value).ToList();
        
        if (type.HasValue)
            vehicles = vehicles.Where(v => v.Type == type.Value).ToList();

        //return dto 
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

        //fleet publishes that the driver is assigned
        //Fleet publishes "driver.assigned"  
        //Trip updates cache: DriverStatus = Assigned
        _publisher.Publish("driver.assigned", new DriverAssignedEvent {
            DriverId = driver.Id,
            VehicleId = vehicle.Id
        });


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


        //fleet publishes that the driver is unassigned
        _publisher.Publish("driver.unassigned", new DriverUnassignedEvent {
            VehicleId = vehicleId,
            DriverId = driver?.Id ?? Guid.Empty 
        });


        return true;
    }
    
    //change the status of a vehicle
    public async Task ChangeVehicleStatus(Guid vehicleId, VehicleStatus status)
    {
        var vehicle = await _repository.GetByIdAsync(vehicleId);//send to repository-get the vehicle by id

        if (vehicle == null)//check if vehicle exists
            throw new Exception("Vehicle not found");//if it does not exist throw validation

        switch (status) //switch case for statuses
        {
            case VehicleStatus.Active: //if we want to change status to active - can only be changed to active if it is registered or in maintenance

                if(vehicle.Status == VehicleStatus.Registered)//see if its status is registered
                    vehicle.Activate(); //call the activate method

                else if(vehicle.Status == VehicleStatus.InMaintenance)//see if its status is in maintenance
                    vehicle.CompleteMaintenance();

                else
                    throw new InvalidStateTransitionException(
                        "Vehicle cannot transition to Active");

                break;

            case VehicleStatus.InMaintenance: //if we want to change status to in maintenance
                vehicle.RequestMaintenance();//allowed if status=active
                break;

            case VehicleStatus.Decommissioned: //if we want to change status to decommissioned
                vehicle.Decommission();//allowed if status=in maintenance
                break;

            case VehicleStatus.Registered:
                throw new Exception("Cannot manually revert to Registered");

            default:
                throw new Exception("Invalid status transition"); //unexpected cases
        }

        //the fleet publishes that the vehicle status changed
        //Fleet publishes "vehicle.status.changed"
        //Trip updates cache: VehicleStatus = Active
        _publisher.Publish("vehicle.status.changed", new VehicleStatusChangedEvent {
            VehicleId = vehicleId,
            Status = status.ToString()
        });



        await _repository.UpdateAsync(vehicle);//send to repository-update vehicle
    }
    
    //update mileage
    public async Task UpdateMileage(Guid vehicleId, decimal distanceKm)
    {
        var vehicle = await _repository.GetByIdAsync(vehicleId);

        if (vehicle == null)
            throw new Exception("Vehicle not found");

        vehicle.UpdateMileage(distanceKm);

        await _repository.UpdateAsync(vehicle);
    }
    
}