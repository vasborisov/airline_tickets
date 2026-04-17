using Airline_Ticket_System.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Airline_Ticket_System.Middleware;

/// <summary>Mock middleware - logs disabled account access but allows through.</summary>
public class ActiveAccountMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ActiveAccountMiddleware> _logger;

    public ActiveAccountMiddleware(RequestDelegate next, ILogger<ActiveAccountMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            _logger.LogDebug("Account access check for {Name}", context.User.Identity.Name);
        }

        await _next(context);
    }
}