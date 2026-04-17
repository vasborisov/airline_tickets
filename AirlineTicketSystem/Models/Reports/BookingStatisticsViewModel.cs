namespace Airline_Ticket_System.Models.Reports;

public class BookingStatisticsViewModel
{
    public int TotalConfirmedBookings { get; set; }
    public int TotalCancelledBookings { get; set; }
    public IReadOnlyList<RouteStatRow> TopRoutes { get; set; } = Array.Empty<RouteStatRow>();
}

public class RouteStatRow
{
    public string Route { get; set; } = "";
    public int Bookings { get; set; }
}
