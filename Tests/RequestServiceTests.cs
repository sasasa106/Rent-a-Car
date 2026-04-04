using Core.Interfaces;
using Core.Projections.Requests;
using Core.Services;
using Data.Models;
using Data.Repositories;
using Moq;

namespace Tests;

public class RequestServiceTests
{
	private readonly Mock<IRepository<Request>> _mockRequestRepository;
	private readonly Mock<IRepository<Car>> _mockCarRepository;
	private readonly IRequestService _requestService;

	public RequestServiceTests()
	{
		_mockRequestRepository = new Mock<IRepository<Request>>();
		_mockCarRepository = new Mock<IRepository<Car>>();
		_requestService = new RequestService(_mockRequestRepository.Object, _mockCarRepository.Object);
	}

	[Fact]
    public void GetByUserProjected_WithValidUserId_ReturnsUserRequests()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var carId = Guid.NewGuid();
        var requests = new List<Request>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CarId = carId,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CarId = Guid.NewGuid(),
                StartDate = DateTime.Now.AddDays(10),
                EndDate = DateTime.Now.AddDays(15)
            }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, RequestProjection>>>()))
            .Returns(requests.Select(req => new RequestProjection
            {
                Id = req.Id,
                CarId = req.CarId,
                UserId = req.UserId,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                DurationDays = (int)(req.EndDate.Date - req.StartDate.Date).TotalDays
            }).AsEnumerable());

        // Act
        var result = _requestService.GetByUserProjected(userId).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(userId, r.UserId));
    }
[Fact]
    public void GetByUserProjected_WithInvalidUserId_ReturnsEmptyList()
    {
        // Arrange
        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, RequestProjection>>>()))
            .Returns(Enumerable.Empty<RequestProjection>());

        // Act
        var result = _requestService.GetByUserProjected(Guid.NewGuid()).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetByCarProjected_WithValidCarId_ReturnsCarRequests()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var requests = new List<Request>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CarId = carId,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CarId = carId,
                StartDate = DateTime.Now.AddDays(10),
                EndDate = DateTime.Now.AddDays(15)
            }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, RequestProjection>>>()))
            .Returns(requests.Select(req => new RequestProjection
            {
                Id = req.Id,
                CarId = req.CarId,
                UserId = req.UserId,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                DurationDays = (int)(req.EndDate.Date - req.StartDate.Date).TotalDays
            }).AsEnumerable());

        // Act
        var result = _requestService.GetByCarProjected(carId).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(carId, r.CarId));
    }
    [Fact]
    public void Constructor_WithNullCarRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RequestService(_mockRequestRepository.Object, null!));
    }

    [Fact]
    public void IsCarAvailable_WithNoConflictingRequests_ReturnsTrue()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var start = DateTime.Now.AddDays(1);
        var end = DateTime.Now.AddDays(5);

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns(Enumerable.Empty<Request>());

        // Act
        var result = _requestService.IsCarAvailable(carId, start, end);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsCarAvailable_WithOverlappingRequest_ReturnsFalse()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var start = DateTime.Now.AddDays(1);
        var end = DateTime.Now.AddDays(5);
        var conflicts = new[]
        {
            new Request
            {
                Id = Guid.NewGuid(),
                CarId = carId,
                UserId = Guid.NewGuid(),
                StartDate = DateTime.Now.AddDays(4),
                EndDate = DateTime.Now.AddDays(6)
            }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>() ))
            .Returns(conflicts);

        // Act
        var result = _requestService.IsCarAvailable(carId, start, end);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CreateRequest_WithValidRequest_ReturnsTrueAndCreatesRequest()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var request = new Request
        {
            Id = Guid.NewGuid(),
            CarId = carId,
            UserId = Guid.NewGuid(),
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(4)
        };

        var car = new Car
        {
            Id = carId,
            Make = "Toyota",
            Model = "Corolla",
            Year = 2024,
            Seats = 5,
            PricePerDay = 70m,
            ImagePath = "/images/corolla.jpg"
        };

        var created = false;
        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>() ))
            .Returns(Enumerable.Empty<Request>());
        _mockCarRepository.Setup(r => r.Get(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>() ))
            .Returns(car);
        _mockRequestRepository.Setup(r => r.Create(It.IsAny<Request>()))
            .Callback<Request>(_ => created = true);

        // Act
        var result = _requestService.CreateRequest(request);

        // Assert
        Assert.True(result);
        Assert.True(created);
    }

    [Fact]
    public void CreateRequest_WithOverlappingRequest_ReturnsFalseAndDoesNotCreate()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var request = new Request
        {
            Id = Guid.NewGuid(),
            CarId = carId,
            UserId = Guid.NewGuid(),
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(4)
        };

        var existing = new Request
        {
            Id = Guid.NewGuid(),
            CarId = carId,
            UserId = Guid.NewGuid(),
            StartDate = DateTime.Now.AddDays(3),
            EndDate = DateTime.Now.AddDays(6)
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>() ))
            .Returns(new[] { existing });

        _mockRequestRepository.Setup(r => r.Create(It.IsAny<Request>()))
            .Callback<Request>(_ => throw new InvalidOperationException("Should not create"));

        // Act
        var result = _requestService.CreateRequest(request);

        // Assert
        Assert.False(result);
    }
[Fact]
    public void CreateRequest_WithMissingCar_ReturnsFalseAndDoesNotCreate()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var request = new Request
        {
            Id = Guid.NewGuid(),
            CarId = carId,
            UserId = Guid.NewGuid(),
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(4)
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>() ))
            .Returns(Enumerable.Empty<Request>());
        _mockCarRepository.Setup(r => r.Get(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>() ))
            .Returns((Car?)null);

        // Act
        var result = _requestService.CreateRequest(request);

        // Assert
        Assert.False(result);
        _mockRequestRepository.Verify(r => r.Create(It.IsAny<Request>()), Times.Never);
    }

    [Fact]
    public void GetTotalRequests_ReturnsCorrectCount()
    {
        // Arrange
        var requests = new[]
        {
            new Request { Id = Guid.NewGuid(), CarId = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(-1), EndDate = DateTime.Now.AddDays(1) },
            new Request { Id = Guid.NewGuid(), CarId = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(-2), EndDate = DateTime.Now.AddDays(-1) }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>() ))
            .Returns(requests);

        // Act
        var result = _requestService.GetTotalRequests();

        // Assert
        Assert.Equal(2, result);
    }
    [Fact]
    public void GetTotalRevenue_ComputesTotalRevenueUsingCarPricePerDay()
    {
        // Arrange
        var car1 = new Car { Id = Guid.NewGuid(), Make = "Audi", Model = "A4", Year = 2023, Seats = 5, PricePerDay = 100m, ImagePath = "/images/a4.jpg" };
        var car2 = new Car { Id = Guid.NewGuid(), Make = "BMW", Model = "X1", Year = 2024, Seats = 5, PricePerDay = 200m, ImagePath = "/images/x1.jpg" };

        var requests = new[]
        {
            new Request { Id = Guid.NewGuid(), CarId = car1.Id, UserId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(4) },
            new Request { Id = Guid.NewGuid(), CarId = car2.Id, UserId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(1) }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>() ))
            .Returns(requests);
        _mockCarRepository.Setup(r => r.Get(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>() ))
            .Returns<System.Linq.Expressions.Expression<System.Func<Car, bool>>>(filter => new[] { car1, car2 }.AsQueryable().FirstOrDefault(filter));

        // Act
        var result = _requestService.GetTotalRevenue();

        // Assert
        Assert.Equal(100m * 3 + 200m * 1, result);
    }
[Fact]
    public void GetRentedCarsNowCount_ReturnsDistinctRentedCarCount()
    {
        // Arrange
        var now = DateTime.Now;
        var carId = Guid.NewGuid();
        var requests = new[]
        {
            new Request { Id = Guid.NewGuid(), CarId = carId, UserId = Guid.NewGuid(), StartDate = now.AddDays(-1), EndDate = now.AddDays(1) },
            new Request { Id = Guid.NewGuid(), CarId = carId, UserId = Guid.NewGuid(), StartDate = now.AddHours(-2), EndDate = now.AddHours(2) },
            new Request { Id = Guid.NewGuid(), CarId = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = now.AddDays(-3), EndDate = now.AddDays(-1) }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>() ))
            .Returns((System.Linq.Expressions.Expression<System.Func<Request, bool>> filter) => requests.AsQueryable().Where(filter).ToList());

        // Act
        var result = _requestService.GetRentedCarsNowCount();

        // Assert
        Assert.Equal(1, result);
    }
[Fact]
    public void GetAvailableCarsNowCount_ReturnsTotalCarsMinusCurrentlyRented()
    {
        // Arrange
        var now = DateTime.Now;
        var carId = Guid.NewGuid();
        var cars = new[]
        {
            new Car { Id = carId, Make = "Toyota", Model = "Corolla", Year = 2024, Seats = 5, PricePerDay = 70m, ImagePath = "/images/corolla.jpg" },
            new Car { Id = Guid.NewGuid(), Make = "Honda", Model = "Civic", Year = 2024, Seats = 5, PricePerDay = 60m, ImagePath = "/images/civic.jpg" }
        };
        var requests = new[]
        {
            new Request { Id = Guid.NewGuid(), CarId = carId, UserId = Guid.NewGuid(), StartDate = now.AddDays(-1), EndDate = now.AddDays(1) }
        };

        _mockCarRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>() ))
            .Returns(cars);
        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>() ))
            .Returns((System.Linq.Expressions.Expression<System.Func<Request, bool>> filter) => requests.AsQueryable().Where(filter).ToList());

        // Act
        var result = _requestService.GetAvailableCarsNowCount();

        // Assert
        Assert.Equal(1, result);
    }


    [Fact]
    public void GetAllProjected_ReturnsAllRequests()
    {
        // Arrange
        var requests = new List<Request>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CarId = Guid.NewGuid(),
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(5)
            }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, RequestProjection>>>()))
            .Returns(requests.Select(req => new RequestProjection
            {
                Id = req.Id,
                CarId = req.CarId,
                UserId = req.UserId,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                DurationDays = (int)(req.EndDate.Date - req.StartDate.Date).TotalDays
            }).AsEnumerable());

        // Act
        var result = _requestService.GetAllProjected().ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    // ===== EDGE CASE TESTS FOR IsCarAvailable =====

    [Fact]
    public void IsCarAvailable_WithAdjacentDates_ReturnsTrue()
    {
        // Arrange - new request starts when existing request ends (should be available)
        var carId = Guid.NewGuid();
        var baseDate = DateTime.Now.Date;
        var existingStart = baseDate.AddDays(1);
        var existingEnd = baseDate.AddDays(5);
        var newStart = existingEnd; // starts exactly when previous ends
        var newEnd = newStart.AddDays(2);

        var existingRequest = new Request
        {
            Id = Guid.NewGuid(),
            CarId = carId,
            UserId = Guid.NewGuid(),
            StartDate = existingStart,
            EndDate = existingEnd
        };

        var allRequests = new[] { existingRequest };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns((System.Linq.Expressions.Expression<System.Func<Request, bool>> filter) => allRequests.AsQueryable().Where(filter).ToList());

        // Act
        var result = _requestService.IsCarAvailable(carId, newStart, newEnd);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsCarAvailable_WithSameDayRental_ReturnsTrue()
    {
        // Arrange - rental for same day with no conflicts
        var carId = Guid.NewGuid();
        var date = DateTime.Now.Date.AddDays(3);

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns(Enumerable.Empty<Request>());

        // Act
        var result = _requestService.IsCarAvailable(carId, date, date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsCarAvailable_WithExactlyOverlappingRequest_ReturnsFalse()
    {
        // Arrange - new request overlaps exactly with existing request
        var carId = Guid.NewGuid();
        var start = DateTime.Now.Date.AddDays(1);
        var end = DateTime.Now.Date.AddDays(5);

        var existingRequest = new Request
        {
            Id = Guid.NewGuid(),
            CarId = carId,
            UserId = Guid.NewGuid(),
            StartDate = start,
            EndDate = end
        };

        var allRequests = new[] { existingRequest };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns((System.Linq.Expressions.Expression<System.Func<Request, bool>> filter) => allRequests.AsQueryable().Where(filter).ToList());

        // Act
        var result = _requestService.IsCarAvailable(carId, start, end);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCarAvailable_WithRequestStartingBeforeExisting_ReturnsFalse()
    {
        // Arrange - new request starts before existing and overlaps end
        var carId = Guid.NewGuid();
        var baseDate = DateTime.Now.Date;
        var existing = new Request
        {
            Id = Guid.NewGuid(),
            CarId = carId,
            UserId = Guid.NewGuid(),
            StartDate = baseDate.AddDays(3),
            EndDate = baseDate.AddDays(5)
        };

        var allRequests = new[] { existing };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns((System.Linq.Expressions.Expression<System.Func<Request, bool>> filter) => allRequests.AsQueryable().Where(filter).ToList());

        // Act
        var result = _requestService.IsCarAvailable(carId, baseDate.AddDays(1), baseDate.AddDays(4));

        // Assert
        Assert.False(result);
    }

    // ===== EDGE CASE TESTS FOR GetTotalRevenue =====

    [Fact]
    public void GetTotalRevenue_WithEmptyRequests_ReturnsZero()
    {
        // Arrange
        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns(Enumerable.Empty<Request>());

        // Act
        var result = _requestService.GetTotalRevenue();

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void GetTotalRevenue_WithSingleDayRental_CalculatesAsOneDay()
    {
        // Arrange - same-day rental should count as 1 day minimum
        var car = new Car
        {
            Id = Guid.NewGuid(),
            Make = "Tesla",
            Model = "Model 3",
            Year = 2024,
            Seats = 5,
            PricePerDay = 150m,
            ImagePath = "/images/model3.jpg"
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            CarId = car.Id,
            UserId = Guid.NewGuid(),
            StartDate = DateTime.Now,
            EndDate = DateTime.Now // same day
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns(new[] { request });
        _mockCarRepository.Setup(r => r.Get(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>()))
            .Returns(car);

        // Act
        var result = _requestService.GetTotalRevenue();

        // Assert
        Assert.Equal(150m, result); // 1 day minimum
    }

    [Fact]
    public void GetTotalRevenue_WithMissingCarReference_SkipsMissingCar()
    {
        // Arrange - request car doesn't exist in system
        var carId1 = Guid.NewGuid();
        var carId2 = Guid.NewGuid();
        var car = new Car
        {
            Id = carId1,
            Make = "Ford",
            Model = "Mustang",
            Year = 2023,
            Seats = 4,
            PricePerDay = 200m,
            ImagePath = "/images/mustang.jpg"
        };

        var requests = new[]
        {
            new Request { Id = Guid.NewGuid(), CarId = carId1, UserId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3) },
            new Request { Id = Guid.NewGuid(), CarId = carId2, UserId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(2) } // missing car
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns(requests);
        _mockCarRepository.Setup(r => r.Get(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>()))
            .Returns<System.Linq.Expressions.Expression<System.Func<Car, bool>>>(
                filter => filter.Compile()(car) ? car : null);

        // Act
        var result = _requestService.GetTotalRevenue();

        // Assert
        Assert.Equal(200m * 2, result); // only counts the request with existing car
    }

    [Fact]
    public void GetTotalRevenue_WithMultipleRequestsSameCar_SumsBothRentals()
    {
        // Arrange - same car rented multiple times
        var car = new Car
        {
            Id = Guid.NewGuid(),
            Make = "Chevrolet",
            Model = "Malibu",
            Year = 2024,
            Seats = 5,
            PricePerDay = 80m,
            ImagePath = "/images/malibu.jpg"
        };

        var requests = new[]
        {
            new Request { Id = Guid.NewGuid(), CarId = car.Id, UserId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3) },
            new Request { Id = Guid.NewGuid(), CarId = car.Id, UserId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(6) }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns(requests);
        _mockCarRepository.Setup(r => r.Get(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>()))
            .Returns(car);

        // Act
        var result = _requestService.GetTotalRevenue();

        // Assert
        Assert.Equal(80m * 2 + 80m * 1, result); // 2 days + 1 day = 240
    }

    // ===== EDGE CASE TESTS FOR GetRentedCarsNowCount =====

    [Fact]
    public void GetRentedCarsNowCount_WithNoCurrentRentals_ReturnsZero()
    {
        // Arrange
        var now = DateTime.Now;
        var requests = new[]
        {
            new Request { Id = Guid.NewGuid(), CarId = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = now.AddDays(-5), EndDate = now.AddDays(-2) },
            new Request { Id = Guid.NewGuid(), CarId = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = now.AddDays(2), EndDate = now.AddDays(5) }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns((System.Linq.Expressions.Expression<System.Func<Request, bool>> filter) => requests.AsQueryable().Where(filter).ToList());

        // Act
        var result = _requestService.GetRentedCarsNowCount();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetRentedCarsNowCount_WithMultipleCarsCurrentlyRented_ReturnsDistinctCount()
    {
        // Arrange - 3 different cars, 2 are currently rented
        var now = DateTime.Now;
        var car1 = Guid.NewGuid();
        var car2 = Guid.NewGuid();
        var car3 = Guid.NewGuid();

        var requests = new[]
        {
            new Request { Id = Guid.NewGuid(), CarId = car1, UserId = Guid.NewGuid(), StartDate = now.AddDays(-1), EndDate = now.AddDays(1) },
            new Request { Id = Guid.NewGuid(), CarId = car1, UserId = Guid.NewGuid(), StartDate = now.AddHours(-2), EndDate = now.AddHours(2) }, // same car, another rental
            new Request { Id = Guid.NewGuid(), CarId = car2, UserId = Guid.NewGuid(), StartDate = now.AddHours(-1), EndDate = now.AddHours(3) },
            new Request { Id = Guid.NewGuid(), CarId = car3, UserId = Guid.NewGuid(), StartDate = now.AddDays(-5), EndDate = now.AddDays(-2) } // past rental
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns((System.Linq.Expressions.Expression<System.Func<Request, bool>> filter) => requests.AsQueryable().Where(filter).ToList());

        // Act
        var result = _requestService.GetRentedCarsNowCount();

        // Assert
        Assert.Equal(2, result); // car1 and car2, but car1 only counts once despite multiple rentals
    }

    [Fact]
    public void GetRentedCarsNowCount_WithRequestStartingToday_CountsAsRented()
    {
        // Arrange - rental starts today and extends into future
        var now = DateTime.Now;
        var requests = new[]
        {
            new Request { Id = Guid.NewGuid(), CarId = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = now, EndDate = now.AddDays(3) }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns((System.Linq.Expressions.Expression<System.Func<Request, bool>> filter) => requests.AsQueryable().Where(filter).ToList());

        // Act
        var result = _requestService.GetRentedCarsNowCount();

        // Assert
        Assert.Equal(1, result);
    }

    // ===== EDGE CASE TESTS FOR GetAvailableCarsNowCount =====

    [Fact]
    public void GetAvailableCarsNowCount_WithAllCarsAvailable_ReturnsTotal()
    {
        // Arrange - no rentals at all
        var cars = new[]
        {
            new Car { Id = Guid.NewGuid(), Make = "Toyota", Model = "Camry", Year = 2024, Seats = 5, PricePerDay = 75m, ImagePath = "/images/camry.jpg" },
            new Car { Id = Guid.NewGuid(), Make = "Honda", Model = "Accord", Year = 2024, Seats = 5, PricePerDay = 70m, ImagePath = "/images/accord.jpg" },
            new Car { Id = Guid.NewGuid(), Make = "Mazda", Model = "Mazda3", Year = 2024, Seats = 5, PricePerDay = 65m, ImagePath = "/images/mazda3.jpg" }
        };

        _mockCarRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>()))
            .Returns(cars);
        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns(Enumerable.Empty<Request>());

        // Act
        var result = _requestService.GetAvailableCarsNowCount();

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void GetAvailableCarsNowCount_WithAllCarsRented_ReturnsZero()
    {
        // Arrange
        var now = DateTime.Now;
        var car1 = Guid.NewGuid();
        var car2 = Guid.NewGuid();

        var cars = new[]
        {
            new Car { Id = car1, Make = "Jeep", Model = "Wrangler", Year = 2024, Seats = 5, PricePerDay = 120m, ImagePath = "/images/wrangler.jpg" },
            new Car { Id = car2, Make = "Nissan", Model = "Altima", Year = 2024, Seats = 5, PricePerDay = 72m, ImagePath = "/images/altima.jpg" }
        };

        var requests = new[]
        {
            new Request { Id = Guid.NewGuid(), CarId = car1, UserId = Guid.NewGuid(), StartDate = now.AddDays(-1), EndDate = now.AddDays(2) },
            new Request { Id = Guid.NewGuid(), CarId = car2, UserId = Guid.NewGuid(), StartDate = now.AddDays(-2), EndDate = now.AddDays(1) }
        };

        _mockCarRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>()))
            .Returns(cars);
        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns((System.Linq.Expressions.Expression<System.Func<Request, bool>> filter) => requests.AsQueryable().Where(filter).ToList());

        // Act
        var result = _requestService.GetAvailableCarsNowCount();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetAvailableCarsNowCount_WithNoCars_ReturnsZero()
    {
        // Arrange
        _mockCarRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>()))
            .Returns(Enumerable.Empty<Car>());

        // Act
        var result = _requestService.GetAvailableCarsNowCount();

        // Assert
        Assert.Equal(0, result);
    }

    // ===== EDGE CASE TESTS FOR GetByUserProjected and GetByCarProjected =====

    [Fact]
    public void GetByUserProjected_CalculatesDurationCorrectly()
    {
        // Arrange - verify duration calculation with various day spans
        var userId = Guid.NewGuid();
        var carId = Guid.NewGuid();
        var start = DateTime.Now.AddDays(1);

        var requests = new List<Request>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, CarId = carId, StartDate = start, EndDate = start.AddDays(5) },
            new() { Id = Guid.NewGuid(), UserId = userId, CarId = carId, StartDate = start.AddDays(10), EndDate = start.AddDays(10) } // same day
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, RequestProjection>>>()))
            .Returns(requests.Select(req => new RequestProjection
            {
                Id = req.Id,
                CarId = req.CarId,
                UserId = req.UserId,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                DurationDays = (int)(req.EndDate.Date - req.StartDate.Date).TotalDays
            }).AsEnumerable());

        // Act
        var result = _requestService.GetByUserProjected(userId).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(5, result[0].DurationDays);
        Assert.Equal(0, result[1].DurationDays);
    }

    [Fact]
    public void GetByCarProjected_WithMultipleRequests_ReturnsOnlyRequestsForCar()
    {
        // Arrange
        var targetCarId = Guid.NewGuid();
        var otherCarId = Guid.NewGuid();

        var requests = new List<Request>
        {
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CarId = targetCarId, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(3) },
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CarId = otherCarId, StartDate = DateTime.Now.AddDays(2), EndDate = DateTime.Now.AddDays(4) },
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CarId = targetCarId, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7) }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, RequestProjection>>>()))
            .Returns(requests
                .Where(req => req.CarId == targetCarId)
                .Select(req => new RequestProjection
                {
                    Id = req.Id,
                    CarId = req.CarId,
                    UserId = req.UserId,
                    StartDate = req.StartDate,
                    EndDate = req.EndDate,
                    DurationDays = (int)(req.EndDate.Date - req.StartDate.Date).TotalDays
                }).AsEnumerable());

        // Act
        var result = _requestService.GetByCarProjected(targetCarId).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(targetCarId, r.CarId));
    }

    // ===== EDGE CASE TESTS FOR Constructor Validation =====

    [Fact]
    public void Constructor_WithNullRequestRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RequestService(null!, _mockCarRepository.Object));
    }

    // ===== EDGE CASE TESTS FOR GetAllProjected =====

    [Fact]
    public void GetAllProjected_WithEmptyRequests_ReturnsEmptyList()
    {
        // Arrange
        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, RequestProjection>>>()))
            .Returns(Enumerable.Empty<RequestProjection>());

        // Act
        var result = _requestService.GetAllProjected().ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllProjected_WithMultipleRequests_CalculatesDurationForEach()
    {
        // Arrange
        var date1 = DateTime.Now.AddDays(1);
        var requests = new List<Request>
        {
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CarId = Guid.NewGuid(), StartDate = date1, EndDate = date1.AddDays(3) },
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CarId = Guid.NewGuid(), StartDate = date1.AddDays(5), EndDate = date1.AddDays(5) },
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CarId = Guid.NewGuid(), StartDate = date1.AddDays(10), EndDate = date1.AddDays(15) }
        };

        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, RequestProjection>>>()))
            .Returns(requests.Select(req => new RequestProjection
            {
                Id = req.Id,
                CarId = req.CarId,
                UserId = req.UserId,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                DurationDays = (int)(req.EndDate.Date - req.StartDate.Date).TotalDays
            }).AsEnumerable());

        // Act
        var result = _requestService.GetAllProjected().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result[0].DurationDays);
        Assert.Equal(0, result[1].DurationDays);
        Assert.Equal(5, result[2].DurationDays);
    }

    [Fact]
    public void GetTotalRequests_WithZeroRequests_ReturnsZero()
    {
        // Arrange
        _mockRequestRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Request, bool>>>()))
            .Returns(Enumerable.Empty<Request>());

        // Act
        var result = _requestService.GetTotalRequests();

        // Assert
        Assert.Equal(0, result);
    }

}
