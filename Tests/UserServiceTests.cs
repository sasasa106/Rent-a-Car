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

	
}
