//controller receive request then calls trip Service ->service ->trip repository saves to db


using Microsoft.AspNetCore.Mvc;
using TripService.Application.Requests;
using TripService.Application.Services;
using TripService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

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
    [Authorize(Roles = "Admin, Dispatcher")] // Dispatchers and Admins can create and assign trips
    [HttpPost]
    public async Task<IActionResult> CreateTrip(CreateTripRequest request)
    {
        var result = await _tripService.CreateTripAsync(request);//send to service
        return Ok(result);
    }
    
    //get all trips
    //with filtering by driver, vehicle, date range, status
    [Authorize(Roles = "Admin, Dispatcher, Driver")] // Drivers can view trips (to see their own)
    [HttpGet]
    public async Task<IActionResult> GetAllTrips([FromQuery] Guid? driverId, [FromQuery] Guid? vehicleId, [FromQuery] TripStatus? status, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var trips = await _tripService.GetAllTripsAsync(driverId, vehicleId, status, startDate, endDate); //send to trip service
        
        return Ok(trips);
    }

    //get trip by id
    [Authorize(Roles = "Admin, Dispatcher, Driver")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrip(Guid id)
    {
        var trip = await _tripService.GetTripByIdAsync(id);

        if (trip == null)//check if null
            return NotFound();

        return Ok(trip);//return trip
    }
    
    //assign trip
    [Authorize(Roles = "Admin, Dispatcher")]
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignTrip(Guid id, AssignTripRequest request)
    {
        await _tripService.AssignTripAsync(id, request);//send to service
        return Ok();
    }

    //start trip
    [Authorize(Roles = "Admin, Dispatcher")]
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartTrip(Guid id)
    {
        await _tripService.StartTripAsync(id);//send to service
        return Ok();
    }

    //complete trip
    [Authorize(Roles = "Admin, Dispatcher")]
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteTrip(Guid id) 
    {
        await _tripService.CompleteTripAsync(id);//send to service-complete trip method
        return Ok();
    }

    //to invoice trip after completion
    [Authorize(Roles = "Admin, Dispatcher")]
    [HttpPost("{id}/invoice")]
    public async Task<IActionResult> InvoiceTrip(Guid id, [FromBody] InvoiceTripRequest request)
    {
        await _tripService.InvoiceTripAsync(id, request.Amount, request.Currency);//send to trip service to invoice trip method
        return Ok();
    }



}