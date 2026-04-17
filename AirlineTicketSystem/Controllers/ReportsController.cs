using Airline_Ticket_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Airline_Ticket_System.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports)
    {
        _reports = reports;
    }

    [HttpGet]
    public async Task<IActionResult> DailyFlights(DateTime? day)
    {
        var d = day ?? DateTime.UtcNow.Date;
        var model = await _reports.GetDailyFlightsAsync(d);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> BookingStatistics()
    {
        var model = await _reports.GetBookingStatisticsAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Financial(DateTime? from, DateTime? to)
    {
        DateTime? fromUtc = from.HasValue ? DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc) : null;
        DateTime? toUtc = to.HasValue ? DateTime.SpecifyKind(to.Value.Date.AddDays(1), DateTimeKind.Utc) : null;
        var model = await _reports.GetFinancialReportAsync(fromUtc, toUtc);
        return View(model);
    }
}
