using Airline_Ticket_System.Models.Reports;

namespace Airline_Ticket_System.Services.Interfaces;

public interface IReportService
{
    Task<DailyFlightsReportViewModel> GetDailyFlightsAsync(DateTime dayUtc, CancellationToken cancellationToken = default);

    Task<BookingStatisticsViewModel> GetBookingStatisticsAsync(CancellationToken cancellationToken = default);

    Task<FinancialReportViewModel> GetFinancialReportAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);
}
