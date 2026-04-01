using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class Request : IIdentifiable
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CarId { get; set; }
        public Car? Car { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // convenience property
        public int DurationDays => (int)(EndDate.Date - StartDate.Date).TotalDays;
    }
}
