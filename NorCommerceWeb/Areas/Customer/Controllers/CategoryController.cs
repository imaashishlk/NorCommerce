using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NorCommerce.DataAccess.Data;
using NorCommerce.Models;
using NorCommerce.Business.Services.IServices;

namespace NorCommerceWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _service;
        public CategoryController(ICategoryService service)
        {
            _service = service;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _service.GetAllCategoriesAsync();
            return View("Index",categories);
        }

        public async Task<IActionResult> Create()
        {
            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePOST(Category category)
        {
            if (!String.IsNullOrEmpty(category.Name) && await _service.IsCategoryNameUniqueAsync(category.Name, null))
            {
                ModelState.AddModelError("Name", "Category already exists.");
            }

            if (ModelState.IsValid) {
                await _service.CreateCategoryAsync(category);
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            
            var category = await _service.GetCategoryByIdAsync(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Update")]
        public async Task<IActionResult> UpdatePOST(Category category)
        {
            if (!String.IsNullOrEmpty(category.Name) && await _service.IsCategoryNameUniqueAsync(category.Name, category.Id))
            {
                ModelState.AddModelError("Name", "Category already exists.");
            }

            if (ModelState.IsValid)
            {
                await _service.UpdateCategoryAsync(category);
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = await _service.GetCategoryByIdAsync(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int id)
        {

            var category = await _service.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            await _service.DeleteCategoryAsync(id);
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");


        }
    }
}
