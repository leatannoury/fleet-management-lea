//used to invoice trip after compleiton

namespace TripService.Application.Requests;


public class InvoiceTripRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}