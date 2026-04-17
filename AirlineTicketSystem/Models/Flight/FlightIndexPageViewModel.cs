namespace Airline_Ticket_System.Models.Flight;

public class FlightIndexPageViewModel
{
    public FlightSearchViewModel Search { get; set; } = new();

    public IReadOnlyList<FlightViewModel> Flights { get; set; } = Array.Empty<FlightViewModel>();

    public int TotalCount { get; set; }

    public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    public int PageSize => Search.PageSize;
}
