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
    [HttpPost]
    public async Task<IActionResult> CreateVehicle(CreateVehicleRequest request)
    {
        var vehicle = await _vehicleService.CreateVehicle(request);
        return Ok(vehicle);
    }

    // GET /api/vehicles
    [HttpGet]
    public async Task<IActionResult> GetVehicles()
    {
        var vehicles = await _vehicleService.GetVehicles();
        return Ok(vehicles);
    }
}