//controller receive request then calls vehicleService ->service creates entity->vehicle repository saves to db

using FleetService.Application.Requests;
using FleetService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly VehicleService _vehicleService;

    public VehiclesController(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    // POST /api/vehicles
    //create/add a vehicle
    [HttpPost]
    public async Task<IActionResult> CreateVehicle(CreateVehicleRequest request)
    {
        var vehicle = await _vehicleService.CreateVehicle(request);//send to service
        return Ok(vehicle);
    }

    // GET /api/vehicles
    //get all vehicles
    [HttpGet]
    public async Task<IActionResult> GetVehicles()
    {
        var vehicles = await _vehicleService.GetVehicles();//send to service
        return Ok(vehicles);
    }
    
    //GET /api/vehicles/{id}
    //get vehicle by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicle(Guid id)
    {
        var vehicle = await _vehicleService.GetVehicle(id); //send to service

        if (vehicle == null) //check if null
            return NotFound();

        return Ok(vehicle);
    }
}