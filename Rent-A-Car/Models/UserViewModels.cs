using System;
using System.ComponentModel.DataAnnotations;

namespace Rent_A_Car.Models
{
    public class UserListViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }

    public class UserCreateViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [RegularExpression("^\\d{10}$", ErrorMessage = "EGN must be exactly 10 digits.")]
        public string EGN { get; set; } = null!;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

    public class UserEditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [MinLength(6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [RegularExpression("^\\d{10}$", ErrorMessage = "EGN must be exactly 10 digits.")]
        public string EGN { get; set; } = null!;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
