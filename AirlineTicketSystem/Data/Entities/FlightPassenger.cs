using System.ComponentModel.DataAnnotations;
using Airline_Ticket_System.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Airline_Ticket_System.Entities
{
    public class FlightPassenger
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public virtual Flight? Flight { get; set; }
        public int PassengerId { get; set; }
        public virtual Passenger? Passenger { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        [Required]
        [MaxLength(6)]
        public string Pnr { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string BookingStatus { get; set; } = "Confirmed";

        public decimal? PaymentAmount { get; set; }

        [MaxLength(20)]
        public string? PaymentStatus { get; set; }

        public DateTime? CancelledAt { get; set; }

        public decimal? RefundAmount { get; set; }
    }
}