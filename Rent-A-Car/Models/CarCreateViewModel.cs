using System.ComponentModel.DataAnnotations;

namespace Rent_A_Car.Models
{
    public class CarCreateViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Make { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = null!;

        [Range(1900, 2100)]
        public int Year { get; set; }

        [Range(1, 20)]
        public int Seats { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PricePerDay { get; set; }

        public IFormFile? Picture { get; set; }
    }
}
