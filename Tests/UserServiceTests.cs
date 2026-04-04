using Core.Interfaces;
using Core.Projections.Users;
using Core.Services;
using Data.Models;
using Data.Repositories;
using Moq;

namespace Tests;

public class UserServiceTests
{
	private readonly Mock<IRepository<User>> _mockUserRepository;
	private readonly IUserService _userService;

	public UserServiceTests()
	{
		_mockUserRepository = new Mock<IRepository<User>>();
		_userService = new UserService(_mockUserRepository.Object);
	}

	[Fact]
    public void GetByUsername_WithValidUsername_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "john_doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890"
        };

        _mockUserRepository.Setup(r => r.Get(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .Returns(user);

        // Act
        var result = _userService.GetByUsername("john_doe");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("john_doe", result.Username);
        Assert.Equal("John", result.FirstName);
    }

    [Fact]
    public void GetByUsername_WithInvalidUsername_ReturnsNull()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.Get(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .Returns((User?)null);

        // Act
        var result = _userService.GetByUsername("nonexistent");

        // Assert
        Assert.Null(result);
    }

}
