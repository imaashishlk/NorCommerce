using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using NorCommerce.Business.Emails;
using NorCommerce.Business.Services.IServices;
using NorCommerce.Models;
using NorCommerce.Models.ViewModels;
using NorCommerce.Utility;
using NorCommerceWeb.Areas.Customer.Controllers;
using Stripe;
using Stripe.Climate;
using System.Security.Claims;


namespace NorCommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        [BindProperty]
        public OrderHeader OrderHeader { get; set; }

        private readonly IOrderService _service;
        private readonly IPaymentService _paymentService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CartController> _logger;


        public OrderController(IOrderService service, IPaymentService paymentService, IEmailSender emailSender, ILogger<CartController> logger)
        {
            _service = service;
            _paymentService = paymentService;
            _emailSender = emailSender;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var orderDetail = await _service.GetOrderByIdAsync(orderId: id, includeUser: true, includeDetails: true);

            return View(orderDetail);
        }

        [AllowAnonymous]
        // GET /order/getall
        public async Task<IActionResult> GetAllAsync(string status)
        {
            string? userId = null;

            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
            }

            var orders = await _service.GetAllOrdersAsync(userId: userId, status: status);
            return Json(new { data = orders });
        }

        public async Task<IActionResult> UpdateOrderDetails(int id)
        {
            var orderFromDb = await _service.GetOrderByIdAsync(orderId: id);

            if (orderFromDb == null)
            {
                TempData["error"] = "Order not found.";
                return RedirectToAction("OrderDetails", new { id = id });
            }

            orderFromDb.Name = OrderHeader.Name;
            orderFromDb.PhoneNumber = OrderHeader.PhoneNumber;
            orderFromDb.StreetAddress = OrderHeader.StreetAddress;
            orderFromDb.City = OrderHeader.City;
            orderFromDb.State = OrderHeader.State;
            orderFromDb.PostalCode = OrderHeader.PostalCode;

            if (orderFromDb.OrderStatus != StaticDetails.StatusPending && orderFromDb.OrderStatus != StaticDetails.StatusApproved)
            {
                orderFromDb.Carrier = OrderHeader.Carrier;
                orderFromDb.TrackingNumber = OrderHeader.TrackingNumber;
            }

            await _service.UpdateOrdersAsync(orderFromDb);

            TempData["success"] = "Order updated successfully.";
            return RedirectToAction("OrderDetails", new { id = orderFromDb.Id });
        }

        public async Task<IActionResult> UpdateStatus(int id, string status, string? carrier = null, string? trackingNumber = null)
        {
            var orderFromDb = await _service.GetOrderByIdAsync(orderId: id, includeUser: true, includeDetails: true);

            if (orderFromDb == null)
            {
                TempData["error"] = "Order not found.";
                return RedirectToAction("OrderDetails", new { id = id });
            }

            switch(status)
            {
                case StaticDetails.StatusInProcess:
                    orderFromDb.OrderStatus = StaticDetails.StatusInProcess;
                    break;
                case StaticDetails.StatusShipped:
                    if (string.IsNullOrEmpty(carrier) || string.IsNullOrEmpty(trackingNumber))
                    {
                        ModelState.AddModelError("Carrier", "Carrier is required.");
                        ModelState.AddModelError("TrackingNumber", "Tracking number is required.");
                        TempData["error"] = "Carrier and Tracking Number are required for shipping.";
                        return View("OrderDetails", orderFromDb);
                    }

                    orderFromDb.ShippingDate = DateTime.Now;
                    orderFromDb.OrderStatus = StaticDetails.StatusShipped;
                    orderFromDb.Carrier = carrier;
                    orderFromDb.TrackingNumber = trackingNumber;

                    var html_order_shipped = EmailTemplates.OrderShipped(orderFromDb);

                    // Sending email
                    try
                    {
                        await _emailSender.SendEmailAsync(
                            orderFromDb.ApplicationUser.Email!,
                            $"Order #{orderFromDb.Id} shipped to your destination.",
                            html_order_shipped);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Order Cancel email failed for order {OrderId}", orderFromDb.Id);
                    }

                    break;
                case StaticDetails.StatusCancelled:
                    if(orderFromDb.OrderStatus == StaticDetails.StatusCancelled || orderFromDb.OrderStatus == StaticDetails.StatusRefunded)
                    {
                        TempData["error"] = "Order already cancelled and refunded.";
                        return RedirectToAction("OrderDetails", new { id = id });
                    } else if (orderFromDb.OrderStatus == StaticDetails.StatusShipped) {
                        TempData["error"] = "Order already shipped and cannot be cancelled.";
                        return RedirectToAction("OrderDetails", new { id = id });
                    }

                    orderFromDb.OrderStatus = StaticDetails.StatusCancelled;
                    break;
                case StaticDetails.StatusRefunded:
                    if (orderFromDb.OrderStatus == StaticDetails.StatusCancelled && orderFromDb.OrderStatus != StaticDetails.StatusRefunded)
                    {
                        if (orderFromDb.PaymentIntentID == null)
                        {
                            TempData["error"] = "Error refunding the payment. Please contact the administrator.";
                            return RedirectToAction("OrderDetails", new { id = id });
                        } else if (orderFromDb.OrderStatus == StaticDetails.StatusShipped)
                        {
                            TempData["error"] = "Shipped order cannot be refunded. Please contact the administrator.";
                            return RedirectToAction("OrderDetails", new { id = id });
                        }

                        try
                        {
                            var refund = await _paymentService.RefundAsync(orderFromDb.PaymentIntentID!);

                            if (refund.Status == "failed")
                            {
                                TempData["error"] = "Refund failed. Please contact the administrator.";
                                return RedirectToAction("OrderDetails", new { id = id });
                            }

                            // "succeeded" or "pending" both count as refund started
                            orderFromDb.PaymentStatus = StaticDetails.PaymentStatusRefunded;
                            orderFromDb.OrderStatus = StaticDetails.StatusRefunded;

                            var html_order_refunded = EmailTemplates.OrderCancelledRefunded(orderFromDb, orderFromDb.OrderDetails);

                            // Sending email
                            try
                            {
                                await _emailSender.SendEmailAsync(
                                    orderFromDb.ApplicationUser.Email!,
                                    $"Order #{orderFromDb.Id} cancelled and refunded",
                                    html_order_refunded);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Order Cancel email failed for order {OrderId}", orderFromDb.Id);
                            }
                        }
                        catch (StripeException ex)
                        {
                            _logger.LogError(ex, "Refund failed for order {OrderId}", orderFromDb.Id);
                            TempData["error"] = $"Refund failed: {ex.StripeError?.Message}";
                            return RedirectToAction("OrderDetails", new { id = orderFromDb.Id });
                        }
                    }
                    else
                    {
                        TempData["error"] = "Order already cancelled and refunded.";
                        return RedirectToAction("OrderDetails", new { id = id });
                    }

                    break;
                default:
                    TempData["error"] = "Invalid status.";
                    return RedirectToAction("OrderDetails", new { id = orderFromDb.Id });
            };
            
            await _service.UpdateOrdersAsync(orderFromDb);

            TempData["success"] = "Order updated successfully.";
            return RedirectToAction("OrderDetails", new { id = orderFromDb.Id });
        }
    }
}
