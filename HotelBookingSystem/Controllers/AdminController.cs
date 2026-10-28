using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var totalRooms = await _context.Rooms.CountAsync();
            var availableRooms = await _context.Rooms.CountAsync(r => r.Status == "Available");
            var maintenanceRooms = await _context.Rooms.CountAsync(r => r.Status == "Maintenance");

            var totalBookings = await _context.Bookings.CountAsync();
            var pendingBookings = await _context.Bookings.CountAsync(b => b.BookingStatus == "Pending");
            var confirmedBookings = await _context.Bookings.CountAsync(b => b.BookingStatus == "Confirmed");
            var checkedInCount = await _context.Bookings.CountAsync(b => b.BookingStatus == "CheckedIn");

            var totalRevenue = await _context.Bookings
                .Where(b => b.BookingStatus != "Cancelled")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0m;

            var totalGuests = await _context.Users.CountAsync(u => u.Role == "Guest");

            double occupancyRate = totalRooms > 0
                ? Math.Round(((double)checkedInCount / totalRooms) * 100, 1)
                : 0.0;

            var recentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .OrderByDescending(b => b.BookingDate)
                .Take(8)
                .ToListAsync();

            var rooms = await _context.Rooms
                .Include(r => r.RoomType)
                .OrderBy(r => r.FloorNumber)
                .ThenBy(r => r.RoomNumber)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TotalBookingsCount = totalBookings,
                PendingBookingsCount = pendingBookings,
                ConfirmedBookingsCount = confirmedBookings,
                CheckedInCount = checkedInCount,
                TotalRoomsCount = totalRooms,
                AvailableRoomsCount = availableRooms,
                MaintenanceRoomsCount = maintenanceRooms,
                TotalGuestsCount = totalGuests,
                OccupancyRate = occupancyRate,
                RecentBookings = recentBookings,
                RoomsList = rooms
            };

            return View(model);
        }

        // GET: /Admin/Bookings
        [HttpGet]
        public async Task<IActionResult> Bookings(string? status, string? search)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(b => b.BookingStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(b =>
                    (b.User != null && (b.User.FullName.ToLower().Contains(search) || b.User.Email.ToLower().Contains(search))) ||
                    (b.Room != null && b.Room.RoomNumber.ToLower().Contains(search)));
            }

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var model = new ManageBookingsFilterViewModel
            {
                Status = status ?? "All",
                SearchQuery = search,
                Bookings = bookings
            };

            return View(model);
        }

        // POST: /Admin/UpdateBookingStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, string newStatus)
        {
            var allowedStatuses = new[] { "Pending", "Confirmed", "CheckedIn", "CheckedOut", "Cancelled" };
            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["ErrorMessage"] = "Invalid booking status.";
                return RedirectToAction(nameof(Bookings));
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction(nameof(Bookings));
            }

            booking.BookingStatus = newStatus;

            // Reflect room occupancy status where applicable
            if (booking.Room != null)
            {
                if (newStatus == "CheckedIn")
                {
                    booking.Room.Status = "Booked";
                }
                else if (newStatus == "CheckedOut" || newStatus == "Cancelled")
                {
                    booking.Room.Status = "Available";
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Booking #{bookingId} status updated to '{newStatus}' successfully.";

            return RedirectToAction(nameof(Bookings));
        }

        // GET: /Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Bookings)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // GET: /Admin/RoomTypes
        [HttpGet]
        public async Task<IActionResult> RoomTypes()
        {
            var types = await _context.RoomTypes
                .Include(rt => rt.Rooms)
                .OrderBy(rt => rt.PricePerNight)
                .ToListAsync();

            return View(types);
        }

        // GET: /Admin/CreateRoomType
        [HttpGet]
        public IActionResult CreateRoomType()
        {
            return View();
        }

        // POST: /Admin/CreateRoomType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoomType(RoomType roomType)
        {
            if (ModelState.IsValid)
            {
                _context.RoomTypes.Add(roomType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Room Type created successfully.";
                return RedirectToAction(nameof(RoomTypes));
            }

            return View(roomType);
        }

        // GET: /Admin/EditRoomType/5
        [HttpGet]
        public async Task<IActionResult> EditRoomType(int id)
        {
            var type = await _context.RoomTypes.FindAsync(id);
            if (type == null)
            {
                return NotFound();
            }

            return View(type);
        }

        // POST: /Admin/EditRoomType/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoomType(int id, RoomType roomType)
        {
            if (id != roomType.RoomTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(roomType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Room Type updated successfully.";
                return RedirectToAction(nameof(RoomTypes));
            }

            return View(roomType);
        }

        // POST: /Admin/DeleteRoomType/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomType(int id)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Rooms)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

            if (roomType == null)
            {
                return NotFound();
            }

            if (roomType.Rooms.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete '{roomType.TypeName}' because it is assigned to {roomType.Rooms.Count} room(s). Reassign them first.";
                return RedirectToAction(nameof(RoomTypes));
            }

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Room Type deleted successfully.";

            return RedirectToAction(nameof(RoomTypes));
        }
    }
}
