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
        // New stats
        public int TotalUsers { get; set; }
        public int UsersRentingNow { get; set; }
        public int ActiveRequestsNow { get; set; }

        public IEnumerable<CarViewModel> Cars { get; set; } = new List<CarViewModel>();
        public IEnumerable<Rent_A_Car.Models.RequestListViewModel> Requests { get; set; } = new List<Rent_A_Car.Models.RequestListViewModel>();
    }
}
