using System;
using System.ComponentModel.DataAnnotations;

namespace Rent_A_Car.Models
{
    public class RentViewModel
    {
        public Guid CarId { get; set; }

        public string CarTitle { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}
