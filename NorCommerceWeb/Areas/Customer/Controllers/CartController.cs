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
    public class CartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;


        public CartController(IShoppingCartService shoppingCartService, IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _shoppingCartService = shoppingCartService;
            _orderService = orderService;
            _userManager = userManager;

        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Unauthorized();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            // Getting all the cart items
            var cartItems = await _shoppingCartService.GetAllByUserAsync(userId);

            var cartModel = new CartVM
            {
                CartItems = cartItems,
                OrderHeader = new OrderHeader
                {
                    ApplicationUserId = userId,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    StreetAddress = user.StreetAddress,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode,
                    OrderTotal = (double) await _shoppingCartService.GetCartTotalAsync(userId)
                },
                CartTotal = await _shoppingCartService.GetCartTotalAsync(userId)
            };

            return View(cartModel);
        }

        public async Task<IActionResult> OrderPlaced(CartVM cartVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.GetUserAsync(User);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (user == null) return Unauthorized();

            var orderHeader = cartVM.OrderHeader;

            // Getting all the cart items
            var cartItems = await _shoppingCartService.GetAllByUserAsync(userId);

            // Mapping shopping cart to OrderDetails
            var orderDetails = cartItems.Select(c => new OrderDetails
            {
                ProductID = c.ProductID,
                Count = c.Count,
                Price = c.Product.Price
            }).ToList();


            orderHeader.OrderDetails = orderDetails;
            orderHeader.ApplicationUserId = userId;
            orderHeader.OrderDate = DateTime.UtcNow;
            orderHeader.OrderTotal = (double) await _shoppingCartService.GetCartTotalAsync(userId);
            orderHeader.OrderStatus = StaticDetails.StatusPending;

            // order create

            var created_order = await _orderService.CreateOrderAsync(orderHeader);

            if (created_order == null)
            {
                TempData["error"] = "Error creating order.";
                return RedirectToAction("Index");
            } else
            {
                // Clear the shopping cart
                await _shoppingCartService.ClearCartAsync(userId);
                TempData["success"] = "Order placed successfully.";
            }

            return RedirectToAction("OrderConfirmed", new { id = created_order.Id });
        }

        public async Task<IActionResult> OrderConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderByIdAsync(id);

            // Limiting to user and if the order belongs to the user
            if (order == null || order.ApplicationUserId != userId)
                return NotFound();

            return View(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _shoppingCartService.RemoveFromCartAsync(productId, userId);

            TempData["success"] = "Removed from cart.";

            return RedirectToAction("Index");
        }
    }
}
