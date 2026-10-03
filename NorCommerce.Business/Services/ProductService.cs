using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NorCommerce.Business.Services.IServices;
using NorCommerce.DataAccess.Data;
using NorCommerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

namespace NorCommerce.Business.Services
{
    public class ProductService : IProductService
    {
        protected readonly ApplicationDbContext _context;
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Product?> GetProductByIdAsync(int id, bool includeCategories = false)
        {
            if (includeCategories)
            {
                return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            }
            else
            {
                return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            }
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(bool includeCategories = false)
        {
            if (includeCategories)
            {
                return await _context.Products.Include(p => p.Category).ToListAsync();
            }
            else
            {
                return await _context.Products.ToListAsync();
            }
        }

        public async Task<Product> CreateProductAsync(Product Product)
        {
            _context.Products.Add(Product);
            await _context.SaveChangesAsync();
            return Product;
        }

        public async Task UpdateProductAsync(Product Product)
        {
            _context.Products.Update(Product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var Product = await _context.Products.FindAsync(id);
            if (Product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            _context.Products.Remove(Product);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> GetAllWithCategories()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }
    }
}
