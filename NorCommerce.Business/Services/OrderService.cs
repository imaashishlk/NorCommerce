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
using Stripe.Checkout;

namespace NorCommerce.Business.Services
{
    public class OrderService : IOrderService
    {
        protected readonly ApplicationDbContext _context;
        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderHeader> CreateOrderAsync(OrderHeader orderHeader)
        {
            await _context.OrderHeaders.AddAsync(orderHeader);
            await _context.SaveChangesAsync();
            return orderHeader;
        }

        public async Task<OrderHeader?> GetOrderByIdAsync(int orderId, bool includeUser = false, bool includeDetails = false)
        {
            var query = _context.OrderHeaders.AsQueryable();

            if (includeUser)
            {
                query = query.Include(u => u.ApplicationUser);
            }

            if (includeDetails)
            {
                query = query.Include(u => u.OrderDetails).ThenInclude(u => u.Product);
            }

            return await query.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<OrderHeader>> GetAllOrdersAsync(string? userId = null, string? status = null, bool includeUser = false, bool includeDetails = false)
        {
            var query = _context.OrderHeaders.AsQueryable();

            if (includeUser)
            {
                query = query.Include(u => u.ApplicationUser);
            }

            if (includeDetails)
            {
                query = query.Include(u => u.OrderDetails).ThenInclude(u => u.Product);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(o => o.ApplicationUserId == userId);
            }

            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            return await query.ToListAsync();
        }

        public async Task UpdateOrdersAsync(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Update(orderHeader);
            await _context.SaveChangesAsync();
        }


    }
}
