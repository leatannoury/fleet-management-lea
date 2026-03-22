//controller receive request then calls vehicleService ->service creates entity->vehicle repository saves to db

using FleetService.Application.Requests;
using FleetService.Application.Services;
using FleetService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FleetService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly DriverService _driverService;

    public DriversController(DriverService driverService)
    {
        _driverService = driverService;
    }

    // POST /api/drivers
    //create a driver
    [Authorize(Roles = "Admin, FleetManager")]
    [HttpPost]
    public async Task<IActionResult> CreateDriver(CreateDriverRequest request)
    {
        var driver = await _driverService.CreateDriver(request);//send to service
        return Ok(driver);
    }

    // GET /api/drivers
    //get all drivers
    [Authorize(Roles = "Admin, FleetManager, Dispatcher")] // Dispatchers need read-only access
    [HttpGet]
    public async Task<IActionResult> GetDrivers()
    {
        var drivers = await _driverService.GetDrivers();//send to service
        return Ok(drivers);
    }
    
    //GET /api/drivers/{id}
    //get driver by id
    //with filtering on driver status
    [Authorize(Roles = "Admin, FleetManager, Dispatcher")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDrivers([FromQuery] DriverStatus? status)
    {
        var drivers = await _driverService.GetDrivers(status); //send to service
        return Ok(drivers);
    }
    
    //PUT /api/drivers/{id}
    //update driver
    [Authorize(Roles = "Admin, FleetManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDriver(Guid id, UpdateDriverRequest request)
    {
        var driver = await _driverService.UpdateDriver(id, request);//send to service

        if (driver == null)//check if null
            return NotFound();

        return Ok(driver);
    }
    
    //DELETE /api/drivers/{id}
    //delete driver
    [Authorize(Roles = "Admin, FleetManager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDriver(Guid id)
    {
        var result = await _driverService.DeleteDriver(id);//send to service

        if (!result)//check if null
            return NotFound();

        return NoContent();
    }
}