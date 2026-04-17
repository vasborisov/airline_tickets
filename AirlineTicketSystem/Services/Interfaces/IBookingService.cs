using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Models.Booking;
using System.Security.Claims;

namespace Airline_Ticket_System.Services.Interfaces;

/// <summary>
/// Application use-case: create a booking with transactional integrity and concurrency checks.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Attempts to persist a booking. Caller is responsible for MVC model validation (data annotations) before calling.
    /// </summary>
    Task<BookingCommitResult> TryCommitBookingAsync(BookSeatViewModel model, ClaimsPrincipal user, ApplicationUser? currentUser, CancellationToken cancellationToken = default);

    Task<CancelBookingResult> TryCancelBookingAsync(int flightPassengerId, ClaimsPrincipal user, ApplicationUser? currentUser, CancellationToken cancellationToken = default);
}
