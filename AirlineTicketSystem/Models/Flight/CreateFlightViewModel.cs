using System.ComponentModel.DataAnnotations;

namespace Airline_Ticket_System.Models.Flight
{
    public class CreateFlightViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Departure City")]
        [Required(ErrorMessage = "The Departure City field is required.")]
        public string DepartureCity { get; set; }

        [Display(Name = "Arrival City")]
        [Required(ErrorMessage = "The Arrival City field is required.")]
        public string ArrivalCity { get; set; }

        [Display(Name = "Duration (in minutes)")]
        [Required(ErrorMessage = "The Duration field is required.")]
        [Range(30, int.MaxValue, ErrorMessage = "Duration must be at least 30 minutes.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "The Price field is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be at least 1.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "The Capacity field is required.")]
        [Display(Name = "Capacity")]
        [Range(5, int.MaxValue, ErrorMessage = "Capacity must be at least 5 minutes.")]
        public int Capacity { get; set; }
    }
}
