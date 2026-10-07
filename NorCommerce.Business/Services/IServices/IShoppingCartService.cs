using System;
using System.Collections.Generic;
using System.Text;
using NorCommerce.Models;

namespace NorCommerce.Business.Services.IServices
{
    public interface IShoppingCartService
    {
        Task<ShoppingCart> AddToCartAsync(ShoppingCart cart);
        Task RemoveFromCartAsync(int productId, string userId);
        Task ClearCartAsync(string userId);
        Task<IEnumerable<ShoppingCart>> GetAllByUserAsync(string userId);
        Task<Decimal> GetCartTotalAsync(string userId);
        Task<int> GetCountAsync(string userId);
    }
}
