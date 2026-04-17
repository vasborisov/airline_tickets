namespace Airline_Ticket_System.Models.Booking;

/// <summary>
/// Outcome of <see cref="Services.Interfaces.IBookingService.TryCommitBookingAsync"/>.
/// </summary>
public sealed class BookingCommitResult
{
    public bool Success { get; init; }

    /// <summary>Error message for ModelState when <see cref="Success"/> is false.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Optional key for ModelState (empty string = general error).</summary>
    public string ErrorKey { get; init; } = string.Empty;

    /// <summary>View model to redisplay when validation failed (dropdowns and flight fields refreshed).</summary>
    public BookSeatViewModel? ViewModel { get; init; }

    /// <summary>6-character booking reference when <see cref="Success"/> is true.</summary>
    public string? Pnr { get; init; }

    public static BookingCommitResult Ok(string? pnr = null) => new() { Success = true, Pnr = pnr };

    public static BookingCommitResult Fail(string message, BookSeatViewModel? viewModel = null, string key = "")
        => new() { Success = false, ErrorMessage = message, ViewModel = viewModel, ErrorKey = key };
}
