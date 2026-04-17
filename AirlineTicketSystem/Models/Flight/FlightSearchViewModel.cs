using System.ComponentModel.DataAnnotations;

namespace Airline_Ticket_System.Models.Flight;

public class FlightSearchViewModel
{
    [Display(Name = "Departure city")]
    public string? DepartureCity { get; set; }

    [Display(Name = "Arrival city")]
    public string? ArrivalCity { get; set; }

    [Display(Name = "From date")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [Display(Name = "To date")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    [Display(Name = "Min price")]
    public decimal? MinPrice { get; set; }

    [Display(Name = "Max price")]
    public decimal? MaxPrice { get; set; }

    [Display(Name = "Flight status")]
    public string? Status { get; set; }

    [Display(Name = "Sort by")]
    public string SortBy { get; set; } = "departure";

    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 15;
}
