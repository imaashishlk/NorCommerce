using System;
using System.Collections.Generic;
using System.Text;
using NorCommerce.Models;

namespace NorCommerce.Business.Services.IServices
{
    public interface IProductService
    {
        Task<Product?> GetProductByIdAsync(int id, bool includeCategories = false);
        Task<IEnumerable<Product>> GetAllProductsAsync(bool includeCategories = false);
        Task<Product> CreateProductAsync(Product Product);
        Task UpdateProductAsync(Product Product);
        Task DeleteProductAsync(int id);

        Task<IEnumerable<Product>> GetAllWithCategories();
    }
}
