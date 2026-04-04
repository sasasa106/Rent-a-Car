using Core.Interfaces;
using Core.Projections.Cars;
using Core.Services;
using Data.Models;
using Data.Repositories;
using Moq;

namespace Tests;

public class CarServiceTests
{
	private readonly Mock<IRepository<Car>> _mockCarRepository;
	private readonly Mock<IRepository<Request>> _mockRequestRepository;
	private readonly ICarService _carService;

	public CarServiceTests()
	{
		_mockCarRepository = new Mock<IRepository<Car>>();
		_mockRequestRepository = new Mock<IRepository<Request>>();
		_carService = new CarService(_mockCarRepository.Object, _mockRequestRepository.Object);
	}
	[Fact]
    public void GetAllProjected_WithValidCars_ReturnsCarListProjections()
    {
        // Arrange
        var cars = new List<Car>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Make = "Toyota",
                Model = "Camry",
                Year = 2024,
                Seats = 5,
                PricePerDay = 50m,
                ImagePath = "/images/camry.jpg"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Make = "Honda",
                Model = "Civic",
                Year = 2023,
                Seats = 5,
                PricePerDay = 45m,
                ImagePath = "/images/civic.jpg"
            }
        };

        _mockCarRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, CarListProjection>>>()))
            .Returns(cars.Select(c => new CarListProjection
            {
                Id = c.Id,
                Make = c.Make,
                Model = c.Model,
                Year = c.Year,
                Seats = c.Seats,
                PricePerDay = c.PricePerDay,
                ImagePath = c.ImagePath
            }).AsEnumerable());

        // Act
        var result = _carService.GetAllProjected().ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Toyota", result[0].Make);
        Assert.Equal("Honda", result[1].Make);
    }
[Fact]
    public void GetAllProjected_WithNoCars_ReturnsEmptyList()
    {
        // Arrange
        _mockCarRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Car, CarListProjection>>>()))
            .Returns(Enumerable.Empty<CarListProjection>());

        // Act
        var result = _carService.GetAllProjected().ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Constructor_WithNullRequestRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CarService(_mockCarRepository.Object, null!));
    }

}
