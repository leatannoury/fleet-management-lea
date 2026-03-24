namespace TripService.Application.Interfaces;

public interface ITripPricingStrategy
{
    //return the calculated cost as decimal
    decimal CalculateCost(string vehicleType, decimal distanceKm, DateTime startTime, DateTime endTime);
}