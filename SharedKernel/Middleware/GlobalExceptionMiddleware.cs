//catch exceptions globally and convert them to clear response

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SharedKernel.Exceptions;

namespace SharedKernel.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next; //next step in pipeline

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); //continue processing the request/ run the req normally/ if exeptions happens go to catch block
        }
        catch (DomainException ex) //domain exception handling. it catches VehicleNotAvailableException, DriverLicenseExpiredException , InvalidStateTransitionException, TripAssignmentException
        {
            await HandleDomainException(context, ex); //pass it to the HandleDomainException method
        }
        catch (Exception ex) //catch unexpected errors
        {
            await HandleUnknownException(context, ex);
        }
    }

    
    //method to handle domain exceptions
    private static Task HandleDomainException(HttpContext context, DomainException ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest; //returns 400
        context.Response.ContentType = "application/json"; //return type is json

        var response = new
        {
            error = ex.Message 
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response)); //return 400 with message
    }

    
    //method to handle unknown exceptions
    private static Task HandleUnknownException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; //returns 500
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "An unexpected error occurred."  //the error message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response)); //return 500 + message
    }
}