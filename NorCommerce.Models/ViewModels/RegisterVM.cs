using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NorCommerce.Models.ViewModels
{
    public class RegisterVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Street Address")]
        public string? StreetAddress { get; set; }

        [Display(Name = "Postal Code")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Postal code must be 4 digits.")]
        public string? PostalCode { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "County")]
        public string? State { get; set; }

        public string Role { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set; }
    }
}
