using System;
using System.ComponentModel.DataAnnotations;

namespace Rent_A_Car.Models
{
    public class CarViewModel
    {
        public Guid Id { get; set; }

        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public int Seats { get; set; }
        public decimal PricePerDay { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
    }
}
