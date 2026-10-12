using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NorCommerce.Business.Services.IServices;
using NorCommerce.DataAccess.Data;
using NorCommerce.Models;
using Microsoft.EntityFrameworkCore;
using NorCommerce.Models.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity;

namespace NorCommerce.Business.Services
{
    public class UserService : IUserService
    {
        protected readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.ApplicationUsers
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            return await _context.ApplicationUsers
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
