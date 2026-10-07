using NorCommerce.Models;
using NorCommerce.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Business.Services.IServices
{
    public interface IOrderService
    {
        Task<OrderHeader> CreateOrderAsync(OrderHeader orderHeader);
        Task<OrderHeader> GetOrderByIdAsync(int orderId);
    }
}
