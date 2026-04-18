using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Airline_Ticket_System.Entities
{
    public class Passenger
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string FamilyName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public virtual ICollection<FlightPassenger> FlightPassengers { get; set; } = new List<FlightPassenger>();

        public Passenger()
        {
        }

        public Passenger(string firstName, string familyName, string? email = null)
        {
            this.FirstName = firstName;
            this.FamilyName = familyName;
            this.Email = email;
        }
    }
}