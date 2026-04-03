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

        // build requests list for admin table
        var requests = _requestService.GetAllProjected()
            .Select(r => {
                    var car = _carService.GetById(r.CarId);
                var user = _userService.GetById(r.UserId);
                var days = (int)(r.EndDate.Date - r.StartDate.Date).TotalDays;
                if (days <= 0) days = 1;
                var price = car != null ? car.PricePerDay * days : 0m;
                return new Rent_A_Car.Models.RequestListViewModel
                {
                    Id = r.Id,
                    CarId = r.CarId,
                    UserId = r.UserId,
                    CarTitle = car != null ? $"{car.Make} {car.Model}" : r.CarId.ToString(),
                    UserEmail = user != null ? user.Email : r.UserId.ToString(),
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    DurationDays = days,
                    TotalPrice = price
                };
            })
            .ToList();

        // Cars list for cars table
        var cars = _carService.GetAllProjected()
            .Select(c => new Rent_A_Car.Models.CarViewModel
            {
                Id = c.Id,
                Make = c.Make,
                Model = c.Model,
                Year = c.Year,
                Seats = c.Seats,
                PricePerDay = c.PricePerDay
            })
            .ToList();

        // additional computed stats
    var totalUsers = users.Count;
    var now = DateTime.UtcNow.Date;
    var activeRequestsNow = requests.Count(r => r.StartDate.Date <= now && r.EndDate.Date >= now);
    var usersRentingNow = requests.Where(r => r.StartDate.Date <= now && r.EndDate.Date >= now)
                     .Select(r => r.UserId)
                     .Distinct()
                     .Count();
    // compute currently rented distinct cars
    var rentedNowComputed = requests.Where(r => r.StartDate.Date <= now && r.EndDate.Date >= now)
                    .Select(r => r.CarId)
                    .Distinct()
                    .Count();
    var availableNow = Math.Max(0, totalCars - rentedNowComputed);

        var vm = new Rent_A_Car.Models.UsersIndexViewModel
        {
            Users = users,
            TotalRequests = totalRequests,
            TotalRevenue = totalRevenue,
            TotalCars = totalCars,
            RentedCarsNow = rentedNowComputed,
            AvailableCarsNow = availableNow,
            Requests = requests,
            Cars = cars,
            TotalUsers = totalUsers,
            ActiveRequestsNow = activeRequestsNow,
            UsersRentingNow = usersRentingNow
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
