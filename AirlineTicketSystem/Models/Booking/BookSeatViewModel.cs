using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Airline_Ticket_System.Models.Booking
{
    public class BookSeatViewModel
    {
        [Display(Name = "Flight Number")]
        public int FlightId { get; set; }

        [Display(Name = "Passenger")]
        public int PassengerId { get; set; }

        [ValidateNever]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [ValidateNever]
        [Display(Name = "Family Name")]
        public string FamilyName { get; set; }

        public int? SelectedPassengerId { get; set; }
        public List<SelectListItem>? ExistingPassengers { get; set; }
        public bool CreateNewPassenger { get; set; }
        public bool IsBookingForSelf { get; set; } = false;

        [ValidateNever]
        public string DepartureCity { get; set; }

        [ValidateNever]
        public string ArrivalCity { get; set; }

        [ValidateNever]
        public int Duration { get; set; }

        [ValidateNever]
        public decimal Price { get; set; }
    }
}
