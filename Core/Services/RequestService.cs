using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Projections.Requests;
using Data.Models;
using Data.Repositories;

namespace Core.Services;

public class RequestService : BaseService<Request>, IRequestService
{
	private readonly IRepository<Car> _carRepository;

	public RequestService(IRepository<Request> repository, IRepository<Car> carRepository) : base(repository)
	{
		this._carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
	}

	public IEnumerable<RequestProjection> GetByUserProjected(Guid userId)
	{
		return this.Repository.GetMany(r => r.UserId == userId, r => new RequestProjection
		{
			Id = r.Id,
			CarId = r.CarId,
			UserId = r.UserId,
			StartDate = r.StartDate,
			EndDate = r.EndDate,
			DurationDays = (int)(r.EndDate.Date - r.StartDate.Date).TotalDays
		});
	}

	public IEnumerable<RequestProjection> GetByCarProjected(Guid carId)
	{
		return this.Repository.GetMany(r => r.CarId == carId, r => new RequestProjection
		{
			Id = r.Id,
			CarId = r.CarId,
			UserId = r.UserId,
			StartDate = r.StartDate,
			EndDate = r.EndDate,
			DurationDays = (int)(r.EndDate.Date - r.StartDate.Date).TotalDays
		});
	}

	public bool IsCarAvailable(Guid carId, DateTime start, DateTime end)
	{
		// A car is available if there are no requests overlapping the requested period
		var conflicts = this.Repository.GetMany(r => r.CarId == carId && !(r.EndDate <= start || r.StartDate >= end));
		return !conflicts.Any();
	}

	public bool CreateRequest(Request request)
	{
		if (!this.IsValid(request)) return false;

		if (!this.IsCarAvailable(request.CarId, request.StartDate, request.EndDate)) return false;

		// Ensure referenced car exists
		var car = this._carRepository.Get(c => c.Id == request.CarId);
		if (car is null) return false;

		this.Repository.Create(request);
		return true;
	}
}
