using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookingsCount { get; set; }
        public int PendingBookingsCount { get; set; }
        public int ConfirmedBookingsCount { get; set; }
        public int CheckedInCount { get; set; }
        public int TotalRoomsCount { get; set; }
        public int AvailableRoomsCount { get; set; }
        public int MaintenanceRoomsCount { get; set; }
        public int TotalGuestsCount { get; set; }
        public double OccupancyRate { get; set; }

        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<Room> RoomsList { get; set; } = new List<Room>();
    }

    public class ManageBookingsFilterViewModel
    {
        public string? Status { get; set; }
        public string? SearchQuery { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
