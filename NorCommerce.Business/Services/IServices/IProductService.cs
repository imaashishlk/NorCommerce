using System;
using System.Collections.Generic;
using System.Text;
using NorCommerce.Models;

namespace NorCommerce.Business.Services.IServices
{
    public interface IProductService
    {
        Task<Product?> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> CreateProductAsync(Product Product);
        Task UpdateProductAsync(Product Product);
        Task DeleteProductAsync(int id);

        Task<IEnumerable<Product>> GetAllWithCategories();
    }
}
