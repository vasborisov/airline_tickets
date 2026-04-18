using Airline_Ticket_System.Entities;

namespace Airline_Ticket_System.Models.Email
{
    public class BookingConfirmationEmailModel
    {
        public string PNR { get; set; } = string.Empty;
        public Flight? Flight { get; set; }
        public Passenger? Passenger { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string PaymentStatus { get; set; } = "Confirmed";
        public string BookingDetailsUrl { get; set; } = string.Empty;
    }
}