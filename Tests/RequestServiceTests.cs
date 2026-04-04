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

	
}
