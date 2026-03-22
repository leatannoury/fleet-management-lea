//controller receive request then calls vehicleService ->service creates entity->vehicle repository saves to db

using FleetService.Application.Requests;
using FleetService.Application.Services;
using FleetService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
    [Authorize(Roles = "Admin, FleetManager")]
    [HttpPost]
    public async Task<IActionResult> CreateVehicle(CreateVehicleRequest request)
    {
        var vehicle = await _vehicleService.CreateVehicle(request);//send to service
        return Ok(vehicle);
    }

    // GET /api/vehicles
    //get all vehicles
    //with accepting filters for vehicle status and vehicle type
    [Authorize(Roles = "Admin, FleetManager, Dispatcher")] // Dispatchers need read-only access
    [HttpGet]
    public async Task<IActionResult> GetVehicles([FromQuery] VehicleStatus? status, [FromQuery] VehicleType? type)
    {
        var vehicles = await _vehicleService.GetVehicles(status, type); //send to service
        return Ok(vehicles);
    }

    
    //GET /api/vehicles/{id}
    //get vehicle by id
    [Authorize(Roles = "Admin, FleetManager, Dispatcher")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicle(Guid id)
    {
        var vehicle = await _vehicleService.GetVehicle(id); //send to service

        if (vehicle == null) //check if null
            return NotFound();

        return Ok(vehicle);
    }
    
    //PUT /api/vehicles/{id}
    //update a vehicle
    [Authorize(Roles = "Admin, FleetManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(Guid id, UpdateVehicleRequest request)
    {
        var vehicle = await _vehicleService.UpdateVehicle(id, request);//send to service

        if (vehicle == null)//check if null
            return NotFound();

        return Ok(vehicle);
    }
    
    //DELETE /api/vehicles/{id}
    //delete a vehicle
    [Authorize(Roles = "Admin, FleetManager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicle(Guid id)
    {
        var result = await _vehicleService.DeleteVehicle(id);//send to service

        if (!result)//check if empty
            return NotFound();

        return NoContent();
    }
    
    //assign driver to vehicle
    [Authorize(Roles = "Admin, FleetManager")]
    [HttpPost("{id}/assign-driver")]
    public async Task<IActionResult> AssignDriver(Guid id, AssignDriverRequest request)
    {
        var result = await _vehicleService.AssignDriver(id, request.DriverId);//send to service

        if (!result)//check if empty
            return NotFound();

        return Ok();
    }
    
    //unassign driver from vehilce
    [Authorize(Roles = "Admin, FleetManager")]
    [HttpPost("{id}/unassign-driver")]
    public async Task<IActionResult> UnassignDriver(Guid id)
    {
        var result = await _vehicleService.UnassignDriver(id);//send to service-unassign driver

        if (!result)//check if empty
            return NotFound();

        

        return Ok();
    }
    
    //change vehicle status
    [Authorize(Roles = "Admin, FleetManager")]
    [HttpPost("{id}/change-status")]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangeVehicleStatusRequest request)
    {
        await _vehicleService.ChangeVehicleStatus(id, request.Status);//send to service

        return Ok();
    }
}