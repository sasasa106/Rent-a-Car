using System;
using Data.Models;

namespace Core.Interfaces;

public interface IRequestService : IService<Request>
{
	IEnumerable<Core.Projections.Requests.RequestProjection> GetByUserProjected(Guid userId);

	IEnumerable<Core.Projections.Requests.RequestProjection> GetByCarProjected(Guid carId);

	bool IsCarAvailable(Guid carId, DateTime start, DateTime end);
    
	// Create a request while performing business checks (availability).
	bool CreateRequest(Request request);
}
