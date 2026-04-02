using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.Interfaces;
using Core.Projections.Users;
using Data.Models;
using Rent_A_Car.Models;

namespace Rent_A_Car.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly Core.Interfaces.IRequestService _requestService;
    private readonly Core.Interfaces.ICarService _carService;

    public UsersController(IUserService userService, Core.Interfaces.IRequestService requestService, Core.Interfaces.ICarService carService)
    {
        _userService = userService;
        _requestService = requestService;
        _carService = carService;
    }

    public IActionResult Index()
    {
        var users = _userService.GetAllProjected()
            .Select(u => new UserListViewModel
            {
                Id = u.Id,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber
            })
            .ToList();

        var totalRequests = _requestService.GetTotalRequests();
        var totalRevenue = _requestService.GetTotalRevenue();
        var totalCars = _carService.GetAllProjected().Count();
        var rentedNow = _requestService.GetRentedCarsNowCount();
        var availableNow = _requestService.GetAvailableCarsNowCount();

        var vm = new Rent_A_Car.Models.UsersIndexViewModel
        {
            Users = users,
            TotalRequests = totalRequests,
            TotalRevenue = totalRevenue,
            TotalCars = totalCars,
            RentedCarsNow = rentedNow,
            AvailableCarsNow = availableNow
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new UserCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserCreateViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        if (_userService.GetByEmail(vm.Email) != null)
        {
            ModelState.AddModelError(nameof(vm.Email), "Email already registered.");
            return View(vm);
        }

        if (_userService.GetByUsername(vm.Username) != null)
        {
            ModelState.AddModelError(nameof(vm.Username), "Username already taken.");
            return View(vm);
        }

        var user = new User
        {
            Username = vm.Username,
            Email = vm.Email,
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            EGN = vm.EGN,
            PhoneNumber = vm.PhoneNumber
        };

        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, vm.Password);

        var created = _userService.Create(user);
        if (!created)
        {
            ModelState.AddModelError(string.Empty, "Unable to create user. Please try again.");
            return View(vm);
        }

        TempData["Success"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(Guid id)
    {
        if (id == Guid.Empty)
            return RedirectToAction(nameof(Index));

        var user = _userService.GetById(id);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new UserEditViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EGN = user.EGN,
            PhoneNumber = user.PhoneNumber
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UserEditViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        if (vm.Id == Guid.Empty)
            return RedirectToAction(nameof(Index));

        var user = _userService.GetById(vm.Id);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(user.Email, vm.Email, StringComparison.OrdinalIgnoreCase)
            && _userService.GetByEmail(vm.Email) != null)
        {
            ModelState.AddModelError(nameof(vm.Email), "Email already registered.");
            return View(vm);
        }

        if (!string.Equals(user.Username, vm.Username, StringComparison.OrdinalIgnoreCase)
            && _userService.GetByUsername(vm.Username) != null)
        {
            ModelState.AddModelError(nameof(vm.Username), "Username already taken.");
            return View(vm);
        }

        user.Username = vm.Username;
        user.Email = vm.Email;
        user.FirstName = vm.FirstName;
        user.LastName = vm.LastName;
        user.EGN = vm.EGN;
        user.PhoneNumber = vm.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(vm.Password))
        {
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, vm.Password);
        }

        var updated = _userService.Update(user);
        if (!updated)
        {
            ModelState.AddModelError(string.Empty, "Unable to update user. Please try again.");
            return View(vm);
        }

        TempData["Success"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["Error"] = "Invalid user ID.";
            return RedirectToAction(nameof(Index));
        }

        var deleted = _userService.Delete(id);
        if (!deleted)
            TempData["Error"] = "User could not be deleted.";
        else
            TempData["Success"] = "User deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
