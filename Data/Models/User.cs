using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class User : IIdentifiable
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        // Store a password hash (not plain text). Validation requires at least 6 characters.
        [Required]
        [MinLength(6)]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        // Bulgarian EGN: 10 digits
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "EGN must be exactly 10 digits.")]
        public string EGN { get; set; } = null!;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        // Navigation: requests made by the user
        public ICollection<Request>? Requests { get; set; }
    }
}
