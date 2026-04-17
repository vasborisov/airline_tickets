using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Airline_Ticket_System.Entities
{
    public class Flight
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Optimistic concurrency token (SQL Server rowversion).</summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }

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

        [Required]
        public DateTime DepartureDateTime { get; set; }

        [Required]
        public DateTime ArrivalDateTime { get; set; }

        [Required]
        [MaxLength(10)]
        public string FlightNumber { get; set; } = string.Empty;

        /// <summary>Scheduled, Delayed, Cancelled, Boarding, Departed.</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Scheduled";

        [MaxLength(10)]
        public string? Gate { get; set; }

        public virtual IList<FlightPassenger> FlightPassengers { get; set; } = new List<FlightPassenger>();

        public Flight()
        {
        }

        public Flight(int id, string departureCity, string arrivalCity, int duration, decimal price, int capacity)
        {
            Id = id;
            DepartureCity = departureCity;
            ArrivalCity = arrivalCity;
            Duration = duration;
            Price = price;
            Capacity = capacity;
            var dep = DateTime.UtcNow.Date.AddHours(8);
            DepartureDateTime = dep;
            ArrivalDateTime = dep.AddMinutes(duration);
            FlightNumber = id > 0 ? $"AT{id:D4}" : "AT0000";
            Status = "Scheduled";
        }
    }
}