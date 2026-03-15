//controller receive request then calls vehicleService ->service creates entity->vehicle repository saves to db

using FleetService.Application.Requests;
using FleetService.Application.Services;
using Microsoft.AspNetCore.Mvc;

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
    [HttpPost]
    public async Task<IActionResult> CreateDriver(CreateDriverRequest request)
    {
        var driver = await _driverService.CreateDriver(request);
        return Ok(driver);
    }

    // GET /api/drivers
    [HttpGet]
    public async Task<IActionResult> GetDrivers()
    {
        var drivers = await _driverService.GetDrivers();
        return Ok(drivers);
    }
}