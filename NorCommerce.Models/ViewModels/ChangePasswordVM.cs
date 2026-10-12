using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NorCommerce.Models.ViewModels
{
    public class ChangePasswordVM
    {
        public string Id { get; set; } = null!;

        [EmailAddress]
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string NewPassword { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
