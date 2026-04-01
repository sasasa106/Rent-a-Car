using System;

namespace Core.Projections.Requests
{
    public class RequestProjection
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
    }
}
