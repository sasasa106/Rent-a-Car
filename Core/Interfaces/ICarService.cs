using System;
using Data.Models;

namespace Core.Interfaces;

public interface ICarService : IService<Car>
{
	IEnumerable<Core.Projections.Cars.CarListProjection> GetAllProjected();

	IEnumerable<Core.Projections.Cars.CarListProjection> Search(string? make, string? model);

	IEnumerable<Core.Projections.Cars.CarListProjection> GetAvailable(DateTime start, DateTime end);
}
