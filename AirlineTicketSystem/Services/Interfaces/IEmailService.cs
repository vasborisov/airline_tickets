using Airline_Ticket_System.Entities;

namespace Airline_Ticket_System.Services.Interfaces;

public interface IEmailService
{
    Task SendWelcomeAsync(string toEmail, string displayName, CancellationToken cancellationToken = default);

    Task SendOperatorCreatedAsync(string toEmail, string displayName, CancellationToken cancellationToken = default);

    Task SendAccountActiveChangedAsync(string toEmail, string displayName, bool isActive, CancellationToken cancellationToken = default);

    Task SendBookingConfirmationAsync(string toEmail, string pnr, Flight flight, Passenger passenger, CancellationToken cancellationToken = default);

    Task SendBookingCancelledAsync(string toEmail, string pnr, decimal? refundAmount, Flight? flight = null, CancellationToken cancellationToken = default);

    Task SendFlightScheduleChangedAsync(string toEmail, Flight flight, DateTime? originalDepartureTime = null, CancellationToken cancellationToken = default);

    Task SendFlightReminderAsync(string toEmail, string pnr, Flight flight, Passenger passenger, CancellationToken cancellationToken = default);
}
