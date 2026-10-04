using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NorCommerce.Models.ViewModels
{
    public class LoginVM
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }

    }
}
