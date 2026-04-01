using System;
using Data.Models;

namespace Core.Interfaces;

public interface IUserService : IService<User>
{
	User? GetByUsername(string username);
	User? GetByEmail(string email);

	IEnumerable<Core.Projections.Users.UserProjection> GetAllProjected();
}
