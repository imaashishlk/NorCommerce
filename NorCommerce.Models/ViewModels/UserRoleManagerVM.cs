using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NorCommerce.Models.ViewModels
{
    public class UserRoleManagerVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public IEnumerable<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();
    }
}
