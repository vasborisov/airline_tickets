namespace Airline_Ticket_System.Repositories.Models;

public sealed class FlightSearchCriteria
{
    public string? DepartureCity { get; init; }
    public string? ArrivalCity { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? Status { get; init; }
    public string SortBy { get; init; } = "departure";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 15;
}
