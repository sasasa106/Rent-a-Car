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

}
