using System.ComponentModel.DataAnnotations;

namespace Rent_A_Car.Models
{
    public class RegisterViewModel
    {
        [Required]
        [MaxLength(50)]
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "EGN must be exactly 10 digits.")]
        public string? EGN { get; set; }
    }
}
