using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Core.Interfaces;
using Data.Models;
using Rent_A_Car.Models;

namespace Rent_A_Car.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(vm);

        var user = _userService.GetByEmail(vm.Email?.Trim() ?? string.Empty);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(vm);
        }

        var hasher = new PasswordHasher<User>();
        var res = hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password ?? string.Empty);
        if (res == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // basic uniqueness checks
        if (!string.IsNullOrWhiteSpace(vm.Email) && _userService.GetByEmail(vm.Email) != null)
        {
            ModelState.AddModelError(nameof(vm.Email), "Email already registered.");
            return View(vm);
        }

        if (!string.IsNullOrWhiteSpace(vm.Username) && _userService.GetByUsername(vm.Username) != null)
        {
            ModelState.AddModelError(nameof(vm.Username), "Username already taken.");
            return View(vm);
        }

        var user = new User
        {
            Username = vm.Username!,
            Email = vm.Email!,
            FirstName = vm.FirstName!,
            LastName = vm.LastName!,
            EGN = vm.EGN!
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, vm.Password ?? string.Empty);

        var created = _userService.Create(user);
        if (!created)
        {
            ModelState.AddModelError(string.Empty, "Unable to create account. Please try again.");
            return View(vm);
        }

        // auto-login after registration
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).GetAwaiter().GetResult();

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }
}
