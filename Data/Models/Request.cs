using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class Request
    {
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }
        public Car? Car { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // convenience property
        public int DurationDays => (int)(EndDate.Date - StartDate.Date).TotalDays;
    }
}
