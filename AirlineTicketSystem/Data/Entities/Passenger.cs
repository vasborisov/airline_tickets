using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Airline_Ticket_System.Entities
{
    public class Passenger
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string FamilyName { get; set; }

        public virtual ICollection<FlightPassenger> FlightPassengers { get; set; }


        public Passenger(string firstName, string familyName)
        {
            this.FirstName = firstName;
            this.FamilyName = familyName;
        }
    }
}