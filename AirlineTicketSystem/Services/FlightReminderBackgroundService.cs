using Airline_Ticket_System.Data;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Airline_Ticket_System.Services;

/// <summary>
/// Background service that sends flight reminder emails 24 hours before departure.
/// This service runs every hour and checks for flights departing in the next 24-25 hours.
/// </summary>
public class FlightReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FlightReminderBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public FlightReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<FlightReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Flight Reminder Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendFlightRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending flight reminders");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Service is being stopped
                break;
            }
        }

        _logger.LogInformation("Flight Reminder Background Service stopped");
    }

    private async Task SendFlightRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Get flights departing in the next 24-25 hours that haven't had reminders sent
        var now = DateTime.UtcNow;
        var reminderWindowStart = now.AddHours(24);
        var reminderWindowEnd = now.AddHours(25);

        var flightsForReminder = await context.Flights
            .Include(f => f.FlightPassengers
                .Where(fp => fp.BookingStatus == "Confirmed"))
                .ThenInclude(fp => fp.Passenger)
            .Where(f => f.DepartureDateTime >= reminderWindowStart && 
                       f.DepartureDateTime <= reminderWindowEnd &&
                       f.Status == "Scheduled" &&
                       f.FlightPassengers.Any(fp => fp.BookingStatus == "Confirmed"))
            .ToListAsync(cancellationToken);

        if (!flightsForReminder.Any())
        {
            _logger.LogDebug("No flights found requiring reminder emails");
            return;
        }

        _logger.LogInformation("Found {Count} flights requiring reminder emails", flightsForReminder.Count);

        foreach (var flight in flightsForReminder)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var confirmedPassengers = flight.FlightPassengers
                .Where(fp => fp.BookingStatus == "Confirmed" && 
                            fp.Passenger != null && 
                            fp.CreatedByUser != null &&
                            !string.IsNullOrEmpty(fp.CreatedByUser.Email))
                .ToList();

            _logger.LogInformation("Sending reminders for flight {FlightNumber} to {PassengerCount} passengers", 
                flight.FlightNumber, confirmedPassengers.Count);

            foreach (var flightPassenger in confirmedPassengers)
            {
                try
                {
                    await emailService.SendFlightReminderAsync(
                        flightPassenger.CreatedByUser!.Email!,
                        flightPassenger.Pnr,
                        flight,
                        flightPassenger.Passenger,
                        cancellationToken);

                    // Small delay to avoid overwhelming the email service
                    await Task.Delay(100, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send flight reminder to {Email} for PNR {PNR}", 
                        flightPassenger.CreatedByUser!.Email, flightPassenger.Pnr);
                }
            }
        }

        _logger.LogInformation("Completed flight reminder processing");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Flight Reminder Background Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}