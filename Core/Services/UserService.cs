using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Core.Interfaces;
using Core.Projections.Users;
using Data.Models;
using Data.Repositories;

namespace Core.Services;

public class UserService : BaseService<User>, IUserService
{
	public UserService(IRepository<User> repository) : base(repository)
	{
	}

	public User? GetByUsername(string username)
	{
		return this.Repository.Get(u => u.Username == username);
	}

	public User? GetByEmail(string email)
	{
		return this.Repository.Get(u => u.Email == email);
	}

	public IEnumerable<UserProjection> GetAllProjected()
	{
		return this.Repository.GetMany(u => true, u => new UserProjection
		{
			Id = u.Id,
			Username = u.Username,
			FirstName = u.FirstName,
			LastName = u.LastName,
			Email = u.Email,
			PhoneNumber = u.PhoneNumber
		});
	}
}
