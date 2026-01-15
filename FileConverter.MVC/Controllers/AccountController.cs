using System.Security.Claims;
using FileConverter.BLL.DTOs.Users;
using FileConverter.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace FileConverter.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
		public IActionResult Login()
		{
			if (User.Identity != null && User.Identity.IsAuthenticated)
				return RedirectToAction("Index", "Plans");

			return View();
		}


		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.LoginAsync(model);

            if (result == null)
            {
                ModelState.AddModelError("", "Email sau parola incorecte.");
                return View(model);
            }

            // AICI e partea de "Admin only"
            // Varianta simplă: verificăm emailul adminului
            // (poți schimba ulterior pe roluri în DB)
            if (!result.Email.Equals("admin@admin.com", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Nu ai acces la Admin Panel.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new Claim(ClaimTypes.Name, result.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Plans");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
