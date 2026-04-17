using Airline_Ticket_System.Models.Reports;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Airline_Ticket_System.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _db;

    public ReportService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DailyFlightsReportViewModel> GetDailyFlightsAsync(DateTime dayUtc, CancellationToken cancellationToken = default)
    {
        var start = dayUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dayUtc.Date, DateTimeKind.Utc)
            : dayUtc.ToUniversalTime().Date;
        var end = start.AddDays(1);

        var flights = await _db.Flights
            .AsNoTracking()
            .Where(f => f.DepartureDateTime >= start && f.DepartureDateTime < end)
            .Include(f => f.FlightPassengers)
            .OrderBy(f => f.DepartureDateTime)
            .ToListAsync(cancellationToken);

        var rows = flights.Select(f =>
        {
            var booked = f.FlightPassengers.Count(p => p.BookingStatus == "Confirmed");
            return new DailyFlightRow
            {
                FlightNumber = f.FlightNumber,
                DepartureCity = f.DepartureCity,
                ArrivalCity = f.ArrivalCity,
                DepartureDateTime = f.DepartureDateTime,
                Status = f.Status,
                BookedSeats = booked,
                TotalSeats = booked + f.Capacity
            };
        }).ToList();

        return new DailyFlightsReportViewModel { DayUtc = start, Flights = rows };
    }

    public async Task<BookingStatisticsViewModel> GetBookingStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var confirmed = await _db.FlightPassengers.CountAsync(fp => fp.BookingStatus == "Confirmed", cancellationToken);
        var cancelled = await _db.FlightPassengers.CountAsync(fp => fp.BookingStatus == "Cancelled", cancellationToken);

        var confirmedRows = await _db.FlightPassengers
            .AsNoTracking()
            .Where(fp => fp.BookingStatus == "Confirmed")
            .Include(fp => fp.Flight)
            .ToListAsync(cancellationToken);

        var top = confirmedRows
            .GroupBy(fp => fp.Flight!.DepartureCity + " → " + fp.Flight!.ArrivalCity)
            .Select(g => new RouteStatRow { Route = g.Key, Bookings = g.Count() })
            .OrderByDescending(x => x.Bookings)
            .Take(10)
            .ToList();

        return new BookingStatisticsViewModel
        {
            TotalConfirmedBookings = confirmed,
            TotalCancelledBookings = cancelled,
            TopRoutes = top
        };
    }

    public async Task<FinancialReportViewModel> GetFinancialReportAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        var q = _db.FlightPassengers.AsNoTracking().Where(fp => fp.BookingStatus == "Confirmed");

        if (fromUtc.HasValue)
            q = q.Where(fp => fp.CreatedAt >= fromUtc.Value);
        if (toUtc.HasValue)
            q = q.Where(fp => fp.CreatedAt < toUtc.Value);

        var list = await q
            .Include(fp => fp.Flight)
            .ToListAsync(cancellationToken);

        var gross = list.Sum(fp => fp.PaymentAmount ?? fp.Flight?.Price ?? 0m);

        var refundQ = _db.FlightPassengers.AsNoTracking().Where(fp => fp.BookingStatus == "Cancelled");
        if (fromUtc.HasValue)
            refundQ = refundQ.Where(fp => fp.CancelledAt >= fromUtc.Value);
        if (toUtc.HasValue)
            refundQ = refundQ.Where(fp => fp.CancelledAt < toUtc.Value);

        var refunds = await refundQ.SumAsync(fp => fp.RefundAmount ?? 0m, cancellationToken);

        var byFlight = list
            .GroupBy(fp => fp.FlightId)
            .Select(g =>
            {
                var first = g.First();
                return new FlightRevenueRow
                {
                    FlightNumber = first.Flight?.FlightNumber ?? "",
                    Route = (first.Flight?.DepartureCity ?? "") + " → " + (first.Flight?.ArrivalCity ?? ""),
                    Revenue = g.Sum(x => x.PaymentAmount ?? x.Flight?.Price ?? 0m)
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        return new FinancialReportViewModel
        {
            FromUtc = fromUtc,
            ToUtc = toUtc,
            GrossSales = gross,
            Refunds = refunds,
            NetRevenue = gross - refunds,
            ByFlight = byFlight
        };
    }
}
