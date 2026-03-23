//controller receive request then calls trip Service ->service ->trip repository saves to db


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TripService.Application.Requests;

[ApiController]
[Route("api/trips/tracking")]
[Authorize] // all tracking endpoints require authentication
public class TripTrackingController : ControllerBase
{
    private readonly TripTrackingService _trackingService;

    public TripTrackingController(TripTrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    // GET api/trips/tracking/{tripId}/logs
    [Authorize(Roles = "Admin, Dispatcher, Driver")]
    [HttpGet("{tripId}/logs")]
    public async Task<IActionResult> GetLogs(Guid tripId)
    {
        var logs = await _trackingService.GetLogsAsync(tripId); //send to service
        return Ok(logs);
    }

    // POST api/trips/tracking/log
    // Only Drivers (and Admin) can submit GPS location logs
    //add location log
    [Authorize(Roles = "Admin, Driver")]
    [HttpPost("log")]
    public async Task<IActionResult> AddLog(AddLocationLogRequest request)
    {
        await _trackingService.AddLocationLogAsync(request);//send to service
        return Ok();
    }

    // POST api/trips/tracking/{tripId}/distance
    // Admin and Dispatcher can query the calculated 
    //calculate distance
    [Authorize(Roles = "Admin, Dispatcher")]
    [HttpPost("{tripId}/distance")]
    public async Task<IActionResult> CalculateDistance(Guid tripId)
    {
        var distance = await _trackingService.CalculateDistanceAsync(tripId);//send to service
        return Ok(distance);
    }


}