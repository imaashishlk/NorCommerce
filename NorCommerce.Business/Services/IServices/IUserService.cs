using NorCommerce.Models;
using NorCommerce.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Business.Services.IServices
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser> GetUserByIdAsync(string id);
    }
}
