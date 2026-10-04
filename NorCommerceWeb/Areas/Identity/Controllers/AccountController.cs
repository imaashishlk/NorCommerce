using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NorCommerce.Models.ViewModels;
using NorCommerce.Models;

namespace NorCommerceWeb.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Register(string? returnURL = null)
        {
            ViewData["ReturnURL"] = returnURL;
            return View();
        }

        [HttpPost]
        [ActionName("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPOST(RegisterVM registerVM, string? returnURL = null)
        {

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = registerVM.Email,
                    Email = registerVM.Email,
                    Name = registerVM.Name,
                    PhoneNumber = registerVM.PhoneNumber,
                    StreetAddress = registerVM.StreetAddress,
                    PostalCode = registerVM.PostalCode,
                    City = registerVM.City,
                    State = registerVM.State
                };

                // Default function, CreateAsync will hash the password and store it securely in the database
                var result = await _userManager.CreateAsync(user, registerVM.Password);

                if (result.Succeeded)
                {
                    // Sign in the user after successful registration
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["Success"] = "Registration successful!";

                    if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
                    {
                        return Redirect(returnURL);
                    }

                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(registerVM);
                }
            }
            return View(registerVM);
        }

        public async Task<IActionResult> Login(string? returnURL = null)
        {
            ViewData["ReturnURL"] = returnURL;
            return View();
        }

        [HttpPost]
        [ActionName("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPOST(LoginVM loginVM, string? returnURL = null)
        {

            if (ModelState.IsValid)
            {
                // Attempt to sign in the user with the provided credentials
                var result = await _signInManager.PasswordSignInAsync(
                    loginVM.Email,
                    loginVM.Password,
                    isPersistent: loginVM.RememberMe, // RememberMe option for persistent login
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    TempData["Success"] = "Logged in successfuly.";

                    if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
                    {
                        return Redirect(returnURL);
                    }

                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
                
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(loginVM);
            }
            return View(loginVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Logged out successfuly.";
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }


        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }

    }
}
