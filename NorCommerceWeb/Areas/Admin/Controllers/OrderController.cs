using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorCommerce.Business.Services.IServices;
using NorCommerce.Models;
using NorCommerce.Models.ViewModels;
using NorCommerce.Utility;
using System.Security.Claims;

namespace NorCommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        [BindProperty]
        public OrderHeader OrderHeader { get; set; }

        private readonly IOrderService _service;
        public OrderController(IOrderService service)
        {
            _service = service;
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
                    break;
                case StaticDetails.StatusCancelled:
                    orderFromDb.OrderStatus = StaticDetails.StatusCancelled;
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
