using System.ComponentModel.DataAnnotations;
using Airline_Ticket_System.Entities;

namespace Airline_Ticket_System.Models.Email
{
    public class BookingConfirmationEmailModel
    {
        [Required]
        public string PNR { get; set; } = string.Empty;
        
        public Flight? Flight { get; set; }
        
        public Passenger? Passenger { get; set; }
        
        public decimal? PaymentAmount { get; set; }
        
        [Required]
        public string PaymentStatus { get; set; } = "Confirmed";
        
        [Required]
        public string BookingDetailsUrl { get; set; } = string.Empty;
    }
}