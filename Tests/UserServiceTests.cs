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

[Fact]
    public void GetByEmail_WithValidEmail_ReturnsUser()
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
        var result = _userService.GetByEmail("john@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public void GetByEmail_WithInvalidEmail_ReturnsNull()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.Get(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .Returns((User?)null);

        // Act
        var result = _userService.GetByEmail("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

[Fact]
    public void GetAllProjected_WithValidUsers_ReturnsUserProjections()
    {
        // Arrange
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Username = "john_doe",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "1234567890"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Username = "jane_smith",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                PhoneNumber = "0987654321"
            }
        };

        _mockUserRepository.Setup(r => r.GetMany(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<System.Func<User, UserProjection>>>()))
            .Returns(users.Select(u => new UserProjection
            {
                Id = u.Id,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber
            }).AsEnumerable());

        // Act
        var result = _userService.GetAllProjected().ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("john_doe", result[0].Username);
        Assert.Equal("jane_smith", result[1].Username);
    }

}
