using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Airline_Ticket_System.Entities
{
    public class Flight
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DepartureCity { get; set; }

        [Required]
        public string ArrivalCity { get; set; }

        [Required]
        [Range(30, int.MaxValue, ErrorMessage = "Duration must be at least 30 minutes.")]
        public int Duration { get; set; } = 30;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Capacity { get; set; }
        public virtual IList<FlightPassenger> FlightPassengers { get; set; } = new List<FlightPassenger>();

        public Flight(int id, string departureCity, string arrivalCity, int duration, decimal price, int capacity)
        {
            this.Id = id;
            this.DepartureCity = departureCity;
            this.ArrivalCity = arrivalCity;
            this.Duration = duration;
            this.Price = price;
            this.Capacity = capacity;
        }
        
    }
}