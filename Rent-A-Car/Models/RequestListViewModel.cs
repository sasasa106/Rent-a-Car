using System;

namespace Rent_A_Car.Models
{
    public class RequestListViewModel
    {
        public Guid Id { get; set; }
        public string CarTitle { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
