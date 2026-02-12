using System.ComponentModel.DataAnnotations;
using Airline_Ticket_System.Data.Entities;

namespace Airline_Ticket_System.Models
{
    public class UserAndRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }

        // Constructor that initializes the Roles property to an empty list
        public UserAndRolesViewModel()
        {
            Roles = new List<string>();
        }
    }
}
