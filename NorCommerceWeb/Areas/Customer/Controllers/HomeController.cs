using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NorCommerce.Business.Services.IServices;
using NorCommerce.Models;
using NorCommerce.Models.ViewModels;
using NorCommerce.Utility;
using System.Security.Claims;

namespace NorCommerceWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;

        public HomeController(IProductService productService, IShoppingCartService shoppingCartService)
        {
            _productService = productService;
            _shoppingCartService = shoppingCartService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync(includeCategories: true);
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId, includeCategories: true);
            if (product == null)
            {
                return NotFound();
            }

            // Initializing a shopping cart
            var shoppingCart = new ShoppingCart
            {
                Product = product,
                ProductID = product.Id,
                Count = 1 // Default count to 1
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ActionName("Details")]
        public async Task<IActionResult> AddToCart(ShoppingCart shoppingCart)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Updating shopping cart with user ID
            shoppingCart.ApplicationUserId = userId;

            var cart = await _shoppingCartService.AddToCartAsync(shoppingCart);

            // Get count and update session
            var count = await _shoppingCartService.GetCountAsync(userId);
            HttpContext.Session.SetInt32(StaticDetails.SessionCart, count);

            TempData["success"] = "Added to cart.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
