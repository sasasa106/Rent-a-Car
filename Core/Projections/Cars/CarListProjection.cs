using System;

namespace Core.Projections.Cars
{
    public class CarListProjection
    {
        public Guid Id { get; set; }
        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public int Seats { get; set; }
        public decimal PricePerDay { get; set; }
        public string? ImagePath { get; set; }
    }
}
