using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rent_A_Car.Models;
using Core.Interfaces;
using Core.Projections.Cars;
using Data.Models;

namespace Rent_A_Car.Controllers;

using Microsoft.AspNetCore.Authorization;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICarService _carService;
    private readonly IRequestService _requestService;
    private readonly IUserService _userService;

    public HomeController(ILogger<HomeController> logger, ICarService carService, IRequestService requestService, IUserService userService)
    {
        _logger = logger;
        _carService = carService;
        _requestService = requestService;
        _userService = userService;
    }

    public IActionResult Index()
    {
        var cars = _carService.GetAllProjected()
            .Select(c => new CarViewModel
            {
                Id = c.Id,
                Make = c.Make,
                Model = c.Model,
                Year = c.Year,
                Seats = c.Seats,
                PricePerDay = c.PricePerDay
            })
            .ToList();

        return View(cars);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View(new CarCreateViewModel());
    }

    [HttpGet]
    public IActionResult Rent(Guid id)
    {
        if (id == Guid.Empty)
            return RedirectToAction(nameof(Index));

        var car = _carService.GetById(id);
        if (car == null)
        {
            TempData["Error"] = "Car not found.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new RentViewModel
        {
            CarId = car.Id,
            CarTitle = $"{car.Make} {car.Model}",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Rent(RentViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        if (vm.StartDate.Date >= vm.EndDate.Date)
        {
            ModelState.AddModelError(string.Empty, "End date must be after start date.");
            return View(vm);
        }

        if (vm.StartDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(string.Empty, "Start date cannot be in the past.");
            return View(vm);
        }

        // check availability
        if (!_requestService.IsCarAvailable(vm.CarId, vm.StartDate, vm.EndDate))
        {
            ModelState.AddModelError(string.Empty, "The car is not available for the selected dates.");
            return View(vm);
        }

        // Require a registered user (by email) to create a booking for now.
        if (string.IsNullOrWhiteSpace(vm.Email))
        {
            ModelState.AddModelError(string.Empty, "Please provide your registered email to complete the booking.");
            return View(vm);
        }

        var user = _userService.GetByEmail(vm.Email.Trim());
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "No registered user found with that email. Please register first.");
            return View(vm);
        }

        var request = new Data.Models.Request
        {
            CarId = vm.CarId,
            UserId = user.Id,
            StartDate = vm.StartDate,
            EndDate = vm.EndDate
        };

        var created = _requestService.CreateRequest(request);
        if (!created)
        {
            TempData["Error"] = "Unable to create booking. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Booking confirmed — the car is locked for your dates.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(CarCreateViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var car = new Car
        {
            Make = vm.Make,
            Model = vm.Model,
            Year = vm.Year,
            Seats = vm.Seats,
            Description = vm.Description,
            PricePerDay = vm.PricePerDay
        };

        var created = _carService.Create(car);
        if (!created)
        {
            ModelState.AddModelError(string.Empty, "Unable to create car. Please check the input.");
            return View(vm);
        }

        TempData["Success"] = "Car added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["Error"] = "Invalid car id.";
            return RedirectToAction(nameof(Index));
        }

        var deleted = _carService.Delete(id);
        if (deleted)
        {
            TempData["Success"] = "Car deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Car could not be deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
