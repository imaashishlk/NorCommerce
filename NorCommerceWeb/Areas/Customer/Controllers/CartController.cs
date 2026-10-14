using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using NorCommerce.Business.Emails;
using NorCommerce.Business.Services;
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
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CartController> _logger;
        private readonly IPaymentService _paymentService;

        public CartController(IShoppingCartService shoppingCartService, IOrderService orderService, UserManager<ApplicationUser> userManager, IEmailSender emailSender, ILogger<CartController> logger, IPaymentService paymentService)
        {
            _shoppingCartService = shoppingCartService;
            _orderService = orderService;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _paymentService = paymentService;
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
            // We check for payment, so Pending for now.
            orderHeader.OrderStatus = StaticDetails.StatusPending;
            orderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;

            // order create

            var created_order = await _orderService.CreateOrderAsync(orderHeader);

            if (created_order == null)
            {
                TempData["error"] = "Error creating order.";
                return RedirectToAction("Index");
            }

            // Now we go with Stripe
            var order = await _orderService.GetOrderByIdAsync(created_order.Id);

            var domain = $"{Request.Scheme}://{Request.Host}/";
            var payment = await _paymentService.CreateCheckoutSessionAsync(
                order!,
                domain + $"Customer/Cart/OrderConfirmed?id={order!.Id}",
                domain + "Customer/Cart/Index");

            order.SessionID = payment.SessionId;
            await _orderService.UpdateOrdersAsync(order);

            return Redirect(payment.Url);
        }

        public async Task<IActionResult> OrderConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderByIdAsync(id, includeDetails: true, includeUser: true);

            // Limiting to user and if the order belongs to the user
            if (order == null || order.ApplicationUserId != userId)
                return NotFound();

            if (order.PaymentStatus != StaticDetails.PaymentStatusApproved && order.SessionID != null)
            {
                var (isPaid, paymentIntentId) = await _paymentService.GetPaymentStatusAsync(order.SessionID);

                if (!isPaid)
                {
                    TempData["error"] = "Payment was not completed.";
                    return RedirectToAction("Index");
                }

                order.PaymentIntentID = paymentIntentId;
                order.OrderStatus = StaticDetails.StatusApproved;
                order.PaymentStatus = StaticDetails.PaymentStatusApproved;
                await _orderService.UpdateOrdersAsync(order);

                // Clear the shopping cart
                await _shoppingCartService.ClearCartAsync(userId);

                // Get count and update session
                var count = await _shoppingCartService.GetCountAsync(userId);
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, count);

                var html = EmailTemplates.OrderConfirmation(order, order.OrderDetails);

                // Sending email
                try
                {
                    await _emailSender.SendEmailAsync(
                        order.ApplicationUser.Email!,
                        $"Order #{order.Id} confirmed",
                        html);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Order email failed for order {OrderId}", order.Id);
                }

                TempData["success"] = "Order placed successfully.";

            }

            return View(order.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _shoppingCartService.RemoveFromCartAsync(productId, userId);

            // Get count and update session
            var count = await _shoppingCartService.GetCountAsync(userId);
            HttpContext.Session.SetInt32(StaticDetails.SessionCart, count);

            TempData["success"] = "Removed from cart.";

            return RedirectToAction("Index");
        }
    }
}
