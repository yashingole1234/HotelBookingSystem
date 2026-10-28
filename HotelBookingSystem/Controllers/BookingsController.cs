using System.Security.Claims;
using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Bookings/Create?roomId=4
        [HttpGet]
        public async Task<IActionResult> Create(int roomId, DateTime? checkIn, DateTime? checkOut, int? guests)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
            {
                TempData["ErrorMessage"] = "The selected room was not found.";
                return RedirectToAction("Index", "Rooms");
            }

            if (room.Status == "Maintenance")
            {
                TempData["ErrorMessage"] = "This room is currently under maintenance and cannot be booked.";
                return RedirectToAction("Index", "Rooms");
            }

            var model = new CreateBookingViewModel
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                RoomTypeName = room.RoomType?.TypeName ?? "Standard Room",
                PricePerNight = room.RoomType?.PricePerNight ?? 0,
                MaxCapacity = room.RoomType?.Capacity ?? 2,
                ImageUrl = room.ImageUrl,
                RoomDescription = room.Description,
                CheckInDate = checkIn ?? DateTime.Today,
                CheckOutDate = checkOut ?? (checkIn?.AddDays(1) ?? DateTime.Today.AddDays(1)),
                NumberOfGuests = guests ?? 1
            };

            return View(model);
        }

        // POST: /Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel model)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomId == model.RoomId);

            if (room == null)
            {
                TempData["ErrorMessage"] = "The selected room is no longer available.";
                return RedirectToAction("Index", "Rooms");
            }

            // Populate view model display properties if re-rendering view
            model.RoomNumber = room.RoomNumber;
            model.RoomTypeName = room.RoomType?.TypeName ?? "Standard";
            model.PricePerNight = room.RoomType?.PricePerNight ?? 0;
            model.MaxCapacity = room.RoomType?.Capacity ?? 2;
            model.ImageUrl = room.ImageUrl;
            model.RoomDescription = room.Description;

            // Date validation
            if (model.CheckInDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("CheckInDate", "Check-in date cannot be in the past.");
            }

            if (model.CheckOutDate.Date <= model.CheckInDate.Date)
            {
                ModelState.AddModelError("CheckOutDate", "Check-out date must be strictly after the check-in date.");
            }

            // Capacity validation
            if (model.NumberOfGuests > (room.RoomType?.Capacity ?? 2))
            {
                ModelState.AddModelError("NumberOfGuests", $"This room accommodates a maximum of {room.RoomType?.Capacity} guests.");
            }

            // Overlap conflict detection (critical hotel business rule)
            bool isOverlapConflict = await _context.Bookings.AnyAsync(b =>
                b.RoomId == model.RoomId &&
                b.BookingStatus != "Cancelled" &&
                model.CheckInDate.Date < b.CheckOutDate.Date &&
                model.CheckOutDate.Date > b.CheckInDate.Date);

            if (isOverlapConflict)
            {
                ModelState.AddModelError("", "This room is already reserved for the selected dates. Please choose different dates or explore other luxury rooms.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get current logged in user ID
            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            int nights = Math.Max(1, (model.CheckOutDate.Date - model.CheckInDate.Date).Days);
            decimal totalAmount = nights * (room.RoomType?.PricePerNight ?? 0);

            var booking = new Booking
            {
                UserId = userId,
                RoomId = model.RoomId,
                CheckInDate = model.CheckInDate.Date,
                CheckOutDate = model.CheckOutDate.Date,
                NumberOfGuests = model.NumberOfGuests,
                TotalAmount = totalAmount,
                BookingStatus = "Confirmed",
                BookingDate = DateTime.Now,
                SpecialRequest = model.SpecialRequest?.Trim()
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation confirmed successfully! We look forward to welcoming you.";
            return RedirectToAction(nameof(Confirmation), new { id = booking.BookingId });
        }

        // GET: /Bookings/Confirmation/5
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Ensure guest only sees their own confirmation (unless Admin)
            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin && int.TryParse(userIdStr, out int userId) && booking.UserId != userId)
            {
                return Forbid();
            }

            var viewModel = new BookingConfirmationViewModel
            {
                BookingId = booking.BookingId,
                GuestName = booking.User?.FullName ?? "Guest",
                GuestEmail = booking.User?.Email ?? string.Empty,
                GuestPhone = booking.User?.Phone ?? "Not Provided",
                RoomNumber = booking.Room?.RoomNumber ?? "N/A",
                RoomTypeName = booking.Room?.RoomType?.TypeName ?? "Luxury Suite",
                PricePerNight = booking.Room?.RoomType?.PricePerNight ?? 0,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                NumberOfGuests = booking.NumberOfGuests,
                TotalAmount = booking.TotalAmount,
                BookingStatus = booking.BookingStatus,
                BookingDate = booking.BookingDate,
                SpecialRequest = booking.SpecialRequest,
                ImageUrl = booking.Room?.ImageUrl
            };

            return View(viewModel);
        }

        // GET: /Bookings/MyBookings
        [HttpGet]
        public async Task<IActionResult> MyBookings(string? filter = "all")
        {
            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .Where(b => b.UserId == userId);

            switch (filter?.ToLower())
            {
                case "upcoming":
                    query = query.Where(b => b.CheckOutDate >= DateTime.Today && b.BookingStatus != "Cancelled");
                    break;
                case "past":
                    query = query.Where(b => b.CheckOutDate < DateTime.Today || b.BookingStatus == "CheckedOut");
                    break;
                case "cancelled":
                    query = query.Where(b => b.BookingStatus == "Cancelled");
                    break;
                default:
                    filter = "all";
                    break;
            }

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            ViewBag.CurrentFilter = filter;
            return View(bookings);
        }

        // POST: /Bookings/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && (!int.TryParse(userIdStr, out int userId) || booking.UserId != userId))
            {
                return Forbid();
            }

            if (booking.BookingStatus == "Cancelled")
            {
                TempData["ErrorMessage"] = "This reservation is already cancelled.";
                return RedirectToAction(nameof(MyBookings));
            }

            if (booking.CheckInDate < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Past or currently checked-in reservations cannot be cancelled online.";
                return RedirectToAction(nameof(MyBookings));
            }

            booking.BookingStatus = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Reservation #{booking.BookingId} has been successfully cancelled.";
            return RedirectToAction(nameof(MyBookings));
        }

        // GET: /Bookings/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && (!int.TryParse(userIdStr, out int userId) || booking.UserId != userId))
            {
                return Forbid();
            }

            return View(booking);
        }
    }
}
