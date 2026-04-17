using System.ComponentModel.DataAnnotations;

namespace Airline_Ticket_System.Models.Flight
{
    public class FlightViewModel
    {
        public int Id { get; set; }
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }

        [Display(Name = "Duration (in minutes)")]
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public bool IsFullyBooked { get; }

        public readonly IList<Passenger.PassengerViewModel>? PassengerViewModels;

        public string? SearchDeparetureCity { get; set; }

        [Display(Name = "Departure (UTC)")]
        public DateTime DepartureDateTime { get; set; }

        [Display(Name = "Flight no.")]
        public string FlightNumber { get; set; } = "";

        public string Status { get; set; } = "Scheduled";

        public string? Gate { get; set; }

        public FlightViewModel(int id, string departureCity, string arrivalCity, int duration, decimal price, int capacity, bool isFullyBooked, IList<Passenger.PassengerViewModel> passengerViewModels)
        {
            Id = id;
            DepartureCity = departureCity;
            ArrivalCity = arrivalCity;
            Duration = duration;
            Price = price;
            Capacity = capacity;
            IsFullyBooked = isFullyBooked;
            PassengerViewModels = passengerViewModels;
        }

        public FlightViewModel(int id, string departureCity, string arrivalCity, int duration, decimal price, int capacity, bool isFullyBooked)
        {
            Id = id;
            DepartureCity = departureCity;
            ArrivalCity = arrivalCity;
            Duration = duration;
            Price = price;
            Capacity = capacity;
            IsFullyBooked = isFullyBooked;
            PassengerViewModels = null;
        }

        public FlightViewModel(int id, string departureCity, string arrivalCity, int duration, decimal price, int capacity)
        {
            Id = id;
            DepartureCity = departureCity;
            ArrivalCity = arrivalCity;
            Duration = duration;
            Price = price;
            Capacity = capacity;
            PassengerViewModels = null;
        }
    }
}
