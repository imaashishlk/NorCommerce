using NorCommerce.Business.Services.IServices;
using NorCommerce.DataAccess.Data;
using NorCommerce.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NorCommerce.Business.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly ApplicationDbContext _context;
        public ShoppingCartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ShoppingCart?> CheckIfExistsAsync(int productId, string userId)
        {
            return await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.ProductID == productId && c.ApplicationUserId == userId);
        }


        public async Task<ShoppingCart> AddToCartAsync(ShoppingCart cart)
        {
            var existingCart = await CheckIfExistsAsync(cart.ProductID, cart.ApplicationUserId);

            if (existingCart == null)
            {
                await _context.ShoppingCarts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }
            else
            {
                existingCart.Count += cart.Count;
                _context.ShoppingCarts.Update(existingCart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<IEnumerable<ShoppingCart>> GetAllByUserAsync(string userId)
        {
            return await _context.ShoppingCarts
                .Where(c => c.ApplicationUserId == userId)
                .Include(c => c.Product)
                .ToListAsync();
        }

        public async Task RemoveFromCartAsync(int productId, string userId)
        {
            var productInCart = await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.ProductID == productId && c.ApplicationUserId == userId);
            if (productInCart != null)
            {
                _context.ShoppingCarts.Remove(productInCart);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Decimal> GetCartTotalAsync(string userId)
        {
            var cartItems = await GetAllByUserAsync(userId);
            return (decimal)cartItems.Sum(item => item.Product.Price * item.Count);
        }

        public async Task<int> GetCountAsync(string userId)
        {
            return await _context.ShoppingCarts
                .Where(c => c.ApplicationUserId == userId)
                .CountAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            await _context.ShoppingCarts
                .Where(c => c.ApplicationUserId == userId)
                .ForEachAsync(c => _context.ShoppingCarts.Remove(c));
            await _context.SaveChangesAsync();
        }
    }
}
