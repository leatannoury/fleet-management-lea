//the Hub is the router for real-time WebSocket traffic. 
//It  receive the GPS points from the Driver, 
//save them to the database using TripTrackingService
// and then broadcast them to Dispatchers

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using TripService.Application.Services;
using TripService.Application.Requests;

namespace TripService.API.Hubs
{
    
    [Authorize] //protect the  hub to ensure no unauthenticated users can connect
    public class TripTrackingHub : Hub
    {
        private readonly TripTrackingService _tripTrackingService;

        //inject trip tracking service
        public TripTrackingHub(TripTrackingService tripTrackingService)
        {
            _tripTrackingService = tripTrackingService;
        }

        // join and leave trip tracking method: for dispatchers & admins
        // they call this to join the room for a specific trip
        [Authorize(Roles = "Admin, Dispatcher")]
        public async Task JoinTripTracking(Guid tripId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, tripId.ToString());
        }

        [Authorize(Roles = "Admin, Dispatcher")]
        public async Task LeaveTripTracking(Guid tripId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tripId.ToString());
        }


        // send location update method: for drivers
        // the driver app calls this rapidly
        [Authorize(Roles = "Admin, Driver")]
        public async Task SendLocationUpdate(Guid tripId, double latitude, double longitude, string address, decimal? speed)
        {
            //first create the request for domain layer
            var request = new AddLocationLogRequest
            {
                TripId = tripId,
                Latitude = latitude,
                Longitude = longitude,
                Address = address ?? "Unknown Address",
                Speed = speed
            };

            //then save the log to db using service
            await _tripTrackingService.AddLocationLogAsync(request);

            //then broadcast this location to dispatchers in the group
            await Clients.Group(tripId.ToString()).SendAsync("ReceiveLocationUpdate", tripId, latitude, longitude, address, speed);
        }
    }
}
