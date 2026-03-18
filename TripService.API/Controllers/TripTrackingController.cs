//controller receive request then calls trip Service ->service ->trip repository saves to db


using Microsoft.AspNetCore.Mvc;
using TripService.Application.Requests;

[ApiController]
[Route("api/trips/tracking")]
public class TripTrackingController : ControllerBase
{
    private readonly TripTrackingService _trackingService;

    public TripTrackingController(TripTrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    //add location log
    [HttpPost("log")]
    public async Task<IActionResult> AddLog(AddLocationLogRequest request)
    {
        await _trackingService.AddLocationLogAsync(request);//send to service
        return Ok();
    }

    //calculate distance
    [HttpPost("{tripId}/distance")]
    public async Task<IActionResult> CalculateDistance(Guid tripId)
    {
        var distance = await _trackingService.CalculateDistanceAsync(tripId);//send to service
        return Ok(distance);
    }
}