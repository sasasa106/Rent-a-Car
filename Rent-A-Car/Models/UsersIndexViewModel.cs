using System.Collections.Generic;

namespace Rent_A_Car.Models
{
    public class UsersIndexViewModel
    {
        public IEnumerable<UserListViewModel> Users { get; set; } = new List<UserListViewModel>();

        public int TotalRequests { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCars { get; set; }
        public int AvailableCarsNow { get; set; }
        public int RentedCarsNow { get; set; }
    }
}
