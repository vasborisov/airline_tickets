namespace Airline_Ticket_System.Models.Booking;

public sealed class CancelBookingResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public decimal? RefundAmount { get; init; }
    public string? Pnr { get; init; }

    public static CancelBookingResult Ok(decimal? refundAmount, string? pnr = null) =>
        new() { Success = true, RefundAmount = refundAmount, Pnr = pnr };

    public static CancelBookingResult Fail(string message) =>
        new() { Success = false, Message = message };
}
