namespace Airline_Ticket_System.Models.Reports;

public class DailyFlightsReportViewModel
{
    public DateTime DayUtc { get; set; }
    public IReadOnlyList<DailyFlightRow> Flights { get; set; } = Array.Empty<DailyFlightRow>();
}

public class DailyFlightRow
{
    public string FlightNumber { get; set; } = "";
    public string DepartureCity { get; set; } = "";
    public string ArrivalCity { get; set; } = "";
    public DateTime DepartureDateTime { get; set; }
    public string Status { get; set; } = "";
    public int BookedSeats { get; set; }
    public int TotalSeats { get; set; }
}
