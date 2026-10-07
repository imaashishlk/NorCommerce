using Microsoft.AspNetCore.Mvc;
using NorCommerce.Models;
using NorCommerce.Models.ViewModels;

namespace NorCommerceWeb.Areas.Customer.Controllers
{
    public class OrderController : Controller
    {
        [Area("Customer")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
