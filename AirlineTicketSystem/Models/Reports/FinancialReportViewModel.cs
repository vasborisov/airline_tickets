namespace Airline_Ticket_System.Models.Reports;

public class FinancialReportViewModel
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public decimal GrossSales { get; set; }
    public decimal Refunds { get; set; }
    public decimal NetRevenue { get; set; }
    public IReadOnlyList<FlightRevenueRow> ByFlight { get; set; } = Array.Empty<FlightRevenueRow>();
}

public class FlightRevenueRow
{
    public string FlightNumber { get; set; } = "";
    public string Route { get; set; } = "";
    public decimal Revenue { get; set; }
}
