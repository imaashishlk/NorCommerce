using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NorCommerce.Models.ViewModels
{
    public class UserVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
    }
}
