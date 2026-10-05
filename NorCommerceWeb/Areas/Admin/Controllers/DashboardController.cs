using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorCommerce.Utility;

namespace NorCommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.RoleAdmin)]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
