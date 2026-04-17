using Microsoft.AspNetCore.Identity;

namespace Airline_Ticket_System.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { set; get; }

        public string FamilyName { set; get; }

        /// <summary>When false, the account cannot sign in (admin-managed).</summary>
        public bool IsActive { get; set; } = true;
    }
}
