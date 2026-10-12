using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NorCommerce.Business.Services.IServices;
using NorCommerce.Models;
using NorCommerce.Models.ViewModels;
using NorCommerce.Utility;
using System.Security.Claims;

namespace NorCommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.RoleAdmin)]
    [AutoValidateAntiforgeryToken]
    public class UserController : Controller
    {
        [BindProperty]
        public ApplicationUser ApplicationUser { get; set; }

        private readonly IUserService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public UserController(IUserService service, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _service = service;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ChangePassword(string id)
        {
            var user = await _service.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(new ChangePasswordVM
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            });
        }

        public async Task<IActionResult> RoleManagement(string id)
        {
            var user = await _service.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Roles
            var roles = await _roleManager.Roles.Select(
                    r => new SelectListItem
                    {
                        Text = r.Name,
                        Value = r.Name
                    }
                ).ToListAsync();

            var userVM = new UserRoleManagerVM
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.UserRoles.FirstOrDefault()?.Role.Name, // assuming user only has one role for now
                RoleList = roles
            };

            return View(userVM);
        }

        [HttpPost]
        [ActionName("RoleManagement")]
        public async Task<IActionResult> RoleManagement(UserRoleManagerVM userRMVM)
        {
            // Here we need only Id and Role, so every other properties are not sent through form in UserRoleManagerVM
            var user = await _service.GetUserByIdAsync(userRMVM.Id);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Not allowing logged in user to change roles
            if (userRMVM.Id == currentUserId)
            {
                TempData["error"] = "Role cannot be assigned to self.";
                return RedirectToAction("Index");
            }

            if (user == null)
            {
                return NotFound();
            }

            var roleExists = await _roleManager.RoleExistsAsync(userRMVM.Role);

            if (string.IsNullOrEmpty(userRMVM.Role) || !roleExists)
            {
                TempData["error"] = "Invalid role.";
                return View(userRMVM);
            }

            var oldRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            if (string.IsNullOrEmpty(oldRole)) return NotFound(); // Redirect, or do something, later.

            await _userManager.UpdateSecurityStampAsync(user); // Logout user if already logged in 
            await _userManager.RemoveFromRoleAsync(user, oldRole);
            await _userManager.AddToRoleAsync(user, userRMVM.Role);

            TempData["success"] = "Role change successful.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ActionName("UserLock")]
        public async Task<IActionResult> UserLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) 
            {
                return NotFound();
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["error"] = "You cannot lock yourself.";
                return RedirectToAction("Index");
            }

            bool isLocked = await _userManager.IsLockedOutAsync(user);

            if (isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null); // Unlock user
                await _userManager.ResetAccessFailedCountAsync(user); // Resetting access failed
                TempData["error"] = "User unlocked.";
                return RedirectToAction("Index");
            } 

            await _userManager.UpdateSecurityStampAsync(user); // Logout user if already logged in 
            await _userManager.SetLockoutEnabledAsync(user, true); // Set lockout enabled
            await _userManager.SetLockoutEndDateAsync(user, DateTime.MaxValue); // Set maximum end date

            TempData["success"] = "User locked.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ActionName("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM cpVM)
        {
            var user = await _userManager.FindByIdAsync(cpVM.Id);

            if (user == null)
            {
                return NotFound();
            }

            bool isLocked = await _userManager.IsLockedOutAsync(user);

            if (isLocked)
            {
                TempData["error"] = "Cannot change password for locked user.";
                return RedirectToAction("Index");
            }

            if (cpVM.NewPassword == cpVM.ConfirmPassword)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, cpVM.ConfirmPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("NewPassword", error.Description);

                    return View(cpVM);
                }

                TempData["success"] = "Password changed successfully.";
            } else
            {
                TempData["error"] = "Passwords do not match.";
            }

            return RedirectToAction("Index");
        }



        // GET /user/getalluser
        public async Task<IActionResult> GetAllUser()
        {
            string? userId = null;

            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
            }

            var users = await _service.GetAllUsersAsync();

            // Filtering fields, don't want everything to show up
            var data = users.Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.PhoneNumber,
                Role = u.UserRoles.FirstOrDefault()?.Role.Name, // assuming user only has one role for now
                Locked = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow
            });

            return Json(new { data });
        }
    }
}
