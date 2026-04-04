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
	
}
