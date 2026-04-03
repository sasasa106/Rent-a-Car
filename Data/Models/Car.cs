using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class Car : IIdentifiable
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Make { get; set; } = null!; // марка

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = null!; // модел

        [Range(1900, 2100)]
        public int Year { get; set; } // година

        [Range(1, 20)]
        public int Seats { get; set; } // брой пасажерски места

        // кратко описание (по избор)
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PricePerDay { get; set; } // цена за наем на ден

        public string? ImagePath { get; set; } // път към изображението

        // Navigation: requests for this car
        public ICollection<Request>? Requests { get; set; }
    }
}
