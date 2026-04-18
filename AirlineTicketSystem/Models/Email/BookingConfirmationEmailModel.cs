using System.ComponentModel.DataAnnotations;

namespace Airline_Ticket_System.Models.Email
{
    public class BookingConfirmationEmailModel
    {
        [Required]
        public string PNR { get; set; } = string.Empty;
        
        public Airline_Ticket_System.Entities.Flight? Flight { get; set; }
        
        public Airline_Ticket_System.Entities.Passenger? Passenger { get; set; }
        
        public decimal? PaymentAmount { get; set; }
        
        [Required]
        public string PaymentStatus { get; set; } = "Confirmed";
        
        [Required]
        public string BookingDetailsUrl { get; set; } = string.Empty;
    }
}