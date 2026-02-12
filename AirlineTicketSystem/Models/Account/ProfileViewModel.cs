using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Airline_Ticket_System.Models.Account
{
    public class ProfileViewModel
    {
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "The First Name field is required.")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s]+$", ErrorMessage = "The First Name field can only contain letters, digits, and spaces.")]
        public required string FirstName { get; set; }

        [Display(Name = "Family Name")]
        [Required(ErrorMessage = "The Family Name field is required.")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s]+$", ErrorMessage = "The Family Name field can only contain letters, digits, and spaces.")]
        public required string FamilyName { get; set; }

        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public required string Email { get; set; }

        [ValidateNever]
        public IList<String> Roles { get; set; }
    }
}
