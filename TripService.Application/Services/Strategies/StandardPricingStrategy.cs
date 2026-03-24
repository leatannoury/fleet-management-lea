using TripService.Application.Interfaces;

namespace TripService.Application.Services.Strategies;

public class StandardPricingStrategy : ITripPricingStrategy
{
    public decimal CalculateCost(string vehicleType, decimal distanceKm, DateTime startTime, DateTime endTime)
    {
        //base rate varies by vehicle type
        decimal baseRate = vehicleType switch
        {
            "Sedan" => 50m,
            "SUV" => 75m,
            "Van" => 100m,
            "Truck" => 150m,
            _ => 50m //default
        };

        //distance-based cost component ($2 per km)
        decimal distanceCost = distanceKm * 2.0m;

        //time-based surcharge for peak hours (e.g., 5 PM to 7 PM = 17:00 to 19:00)
        decimal peakMultiplier = 1.0m;
        if (startTime.Hour >= 17 && startTime.Hour <= 19)
        {
            peakMultiplier = 1.5m; // 50% surcharge
        }

        return (baseRate + distanceCost) * peakMultiplier;
    }
}