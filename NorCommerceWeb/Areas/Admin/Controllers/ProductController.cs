using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using NorCommerce.Business.Services.IServices;
using NorCommerce.DataAccess.Data;
using NorCommerce.Models;
using NorCommerce.Models.ViewModels;
using NorCommerce.Utility;

namespace NorCommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.RoleAdmin)]
    public class ProductController : Controller
    {
        private readonly IProductService _service;
        private readonly ICategoryService _serviceCategory;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IProductService service, ICategoryService serviceCategory, IWebHostEnvironment hostEnvironment)
        {
            _service = service;
            _serviceCategory = serviceCategory;
            _hostEnvironment = hostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _service.GetAllProductsAsync();
            return View("Index",products);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            // List of categories for the dropdown
            var categories = await _serviceCategory.GetAllCategoriesAsync();

            if (id == null || id == 0)
            {
                ProductVM productVM = new ProductVM()
                {
                    Product = new Product(),
                    CategoryList = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    })
                };

                return View("Upsert", productVM);
            }
            else
            {
                var product = await _service.GetProductByIdAsync(id.Value);

                if (product == null)
                {
                    return NotFound();
                }

                ProductVM productVM = new ProductVM()
                {
                    Product = product,
                    CategoryList = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    })
                };
                return View("Upsert", productVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Upsert")]
        public async Task<IActionResult> Upsert(ProductVM productVM, IFormFile? file)
        {          

            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;

                // Handle file upload if a file is provided
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\products");

                    // Delete the old image if it exists
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldPath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(productPath))
                    {
                        Directory.CreateDirectory(productPath);
                    }

                    // Save the new image file
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = "/images/products/" + fileName;
                }

                if (productVM.Product.Id == 0)
                {
                    await _service.CreateProductAsync(productVM.Product);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    await _service.UpdateProductAsync(productVM.Product);
                    TempData["success"] = "Product updated successfully";
                }

                return RedirectToAction("Index");
            }

            var categories = await _serviceCategory.GetAllCategoriesAsync();
            productVM.CategoryList = categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });

            return View(productVM);
        }

        // GET /product/getall
        public async Task<IActionResult> GetAllAsync()
        {
            var products = await _service.GetAllWithCategories();
            return Json(products);
        }

        // DELETE /product/delete/5
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {

            var product = await _service.GetProductByIdAsync(id);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            // Removing the image file
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _service.DeleteProductAsync(id);
            return Json(new { success = true, message = "Product deleted" });
        }
    }

}
