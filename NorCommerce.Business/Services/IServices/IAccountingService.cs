using NorCommerce.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Business.Services.IServices
{
    public interface IAccountingService
    {
        Task<int> GetOrCreateCustomerAsync(ApplicationUser user);
        Task SyncOrderAsync(int orderHeaderId);
        Task<List<int>> GetPendingOrderIdsAsync();
    }
}
