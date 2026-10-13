using Microsoft.AspNetCore.Mvc;
using NorCommerce.Business.Services.IServices;
using NorCommerce.Utility;
using System.Security.Claims;

namespace NorCommerceWeb.Areas.Customer.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly IShoppingCartService _cartService;

        public CartCountViewComponent(IShoppingCartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                HttpContext.Session.Remove(StaticDetails.SessionCart);
                return Content("0");
            }

            var count = HttpContext.Session.GetInt32(StaticDetails.SessionCart);

            if (count == null)
            {
                count = await _cartService.GetCountAsync(userId);
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, count.Value);
            }

            return Content(count.Value.ToString());
        }
    }
}