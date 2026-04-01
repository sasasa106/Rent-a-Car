using System;

namespace Core.Projections.Cars
{
    public class CarDetailProjection : CarListProjection
    {
        public string? Description { get; set; }
    }
}
