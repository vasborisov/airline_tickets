namespace Airline_Ticket_System.Models.User
{
    public class UserViewModel
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string FamilyName { get; set; }

        public string Role { get; set; }

        public UserViewModel(string id, string email, string firstName, string familyName, string role)
        {
            Id = id;
            Email = email;
            FirstName = firstName;
            FamilyName = familyName;
            Role = role;
        }
    }
}
