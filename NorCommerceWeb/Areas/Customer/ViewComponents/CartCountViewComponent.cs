using Microsoft.AspNetCore.Mvc;
using NorCommerce.Business.Services.IServices;
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

            var count = string.IsNullOrEmpty(userId)
                ? 0
                : await _cartService.GetCountAsync(userId);

            return Content(count.ToString());
        }
    }
}