//controller for REST-based trip tracking (HTTP fallback alongside SignalR hub)

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

    // POST api/trips/tracking/log
    // Only Drivers (and Admin) can submit GPS location logs
    [Authorize(Roles = "Admin, Driver")]
    [HttpPost("log")]
    public async Task<IActionResult> AddLog(AddLocationLogRequest request)
    {
        await _trackingService.AddLocationLogAsync(request); //send to service
        return Ok();
    }

    // POST api/trips/tracking/{tripId}/distance
    // Admin and Dispatcher can query the calculated distance
    [Authorize(Roles = "Admin, Dispatcher")]
    [HttpPost("{tripId}/distance")]
    public async Task<IActionResult> CalculateDistance(Guid tripId)
    {
        var distance = await _trackingService.CalculateDistanceAsync(tripId); //send to service
        return Ok(distance);
    }
}