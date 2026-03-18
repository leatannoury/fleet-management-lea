//controller receive request then calls trip Service ->service ->trip repository saves to db


using Microsoft.AspNetCore.Mvc;
using TripService.Application.Requests;
using TripService.Application.Services;
using TripService.Domain.Enums;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly TripService.Application.Services.TripService _tripService;

    public TripsController(TripService.Application.Services.TripService tripService)
    {
        _tripService = tripService;
    }

    //create a trip
    [HttpPost]
    public async Task<IActionResult> CreateTrip(CreateTripRequest request)
    {
        var result = await _tripService.CreateTripAsync(request);//send to service
        return Ok(result);
    }
    
    //get all trips
    //with filtering by driver, vehicle, date range, status
    [HttpGet]
    public async Task<IActionResult> GetAllTrips([FromQuery] Guid? driverId, [FromQuery] Guid? vehicleId, [FromQuery] TripStatus? status, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var trips = await _tripService.GetAllTripsAsync(driverId, vehicleId, status, startDate, endDate); //send to trip service
        
        return Ok(trips);
    }

    //get trip by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrip(Guid id)
    {
        var trip = await _tripService.GetTripByIdAsync(id);

        if (trip == null)//check if null
            return NotFound();

        return Ok(trip);//return trip
    }
    
    //assign trip
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignTrip(Guid id, AssignTripRequest request)
    {
        await _tripService.AssignTripAsync(id, request);//send to service
        return Ok();
    }

    //start trip
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartTrip(Guid id)
    {
        await _tripService.StartTripAsync(id);//send to service
        return Ok();
    }

    //complete trip
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteTrip(Guid id) 
    {
        await _tripService.CompleteTripAsync(id);//send to service-complete trip method
        return Ok();
    }

}