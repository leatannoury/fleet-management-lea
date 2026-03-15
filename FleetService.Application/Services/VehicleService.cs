//service
//implement use cases of the system
//controller calls it
//architecture: controller->application service->domain entity

using FleetService.Application.DTOs;
using FleetService.Application.Requests;
using FleetService.Domain.Entities;
using FleetService.Domain.Enums;
using SharedKernel.ValueObjects;

namespace FleetService.Application.Services;

public class VehicleService
{
    private readonly List<Vehicle> _vehicles = new();//temporary now later postgre

    public VehicleDto CreateVehicle(CreateVehicleRequest request)
    {
        //controller receives json, it becomes CreateVehicleRequest, the service convert it to a domain entity, and return a dto
        
        var vehicle = new Vehicle(
            new PlateNumber(request.PlateNumber),
            request.Model,
            request.Year,
            request.Type
        );

        _vehicles.Add(vehicle);

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

    public List<VehicleDto> GetVehicles()
    {
        return _vehicles.Select(v => new VehicleDto
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
}