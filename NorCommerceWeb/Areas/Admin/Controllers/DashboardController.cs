using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorCommerce.Business.Services.IServices;
using NorCommerce.Utility;

namespace NorCommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.RoleAdmin)]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dashboardService.GetDashboardAsync());
        }
    }
}
