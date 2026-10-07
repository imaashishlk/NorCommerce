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

        public async Task<OrderHeader?> GetOrderByIdAsync(int orderId)
        {
            return await _context.OrderHeaders.FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}
