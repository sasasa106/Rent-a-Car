using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Projections.Cars;
using Data.Models;
using Data.Repositories;

namespace Core.Services;

public class CarService : BaseService<Car>, ICarService
{
	private readonly Data.Repositories.IRepository<Request> _requestRepository;

	public CarService(IRepository<Car> repository, Data.Repositories.IRepository<Request> requestRepository) : base(repository)
	{
		this._requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
	}

	public IEnumerable<CarListProjection> GetAllProjected()
	{
		return this.Repository.GetMany(c => true, c => new CarListProjection
		{
			Id = c.Id,
			Make = c.Make,
			Model = c.Model,
			Year = c.Year,
			Seats = c.Seats,
			PricePerDay = c.PricePerDay,
			ImagePath = c.ImagePath,
			Description = c.Description
		});
	}

	public IEnumerable<CarListProjection> Search(string? make, string? model)
	{
		make = string.IsNullOrWhiteSpace(make) ? null : make;
		model = string.IsNullOrWhiteSpace(model) ? null : model;

		return this.Repository.GetMany(
			c => (make == null || c.Make.Contains(make)) && (model == null || c.Model.Contains(model)),
			c => new CarListProjection
			{
				Id = c.Id,
				Make = c.Make,
				Model = c.Model,
				Year = c.Year,
				Seats = c.Seats,
				PricePerDay = c.PricePerDay,
				ImagePath = c.ImagePath,
				Description = c.Description
			});
	}

	public IEnumerable<CarListProjection> GetAvailable(DateTime start, DateTime end)
	{
		// A car is available if it has no requests that overlap the requested period.
		// We query Cars and project; repository doesn't support joins, so we'll fetch car ids that have conflicting requests and filter.
		var conflictingRequests = this._requestRepository
			.GetMany(r => !(r.EndDate <= start || r.StartDate >= end))
			.Select(r => r.CarId)
			.ToHashSet();

		return this.Repository.GetMany(c => !conflictingRequests.Contains(c.Id), c => new CarListProjection
		{
			Id = c.Id,
			Make = c.Make,
			Model = c.Model,
			Year = c.Year,
			Seats = c.Seats,
			PricePerDay = c.PricePerDay,
			ImagePath = c.ImagePath,
			Description = c.Description
		});
	}
}
