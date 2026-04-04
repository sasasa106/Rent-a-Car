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

}
