using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Rooms
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? typeId, decimal? maxPrice, int? guests, string? status, DateTime? checkIn, DateTime? checkOut)
        {
            var query = _context.Rooms
                .Include(r => r.RoomType)
                .AsQueryable();

            if (typeId.HasValue && typeId.Value > 0)
            {
                query = query.Where(r => r.RoomTypeId == typeId.Value);
            }

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(r => r.RoomType != null && r.RoomType.PricePerNight <= maxPrice.Value);
            }

            if (guests.HasValue && guests.Value > 0)
            {
                query = query.Where(r => r.RoomType != null && r.RoomType.Capacity >= guests.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(r => r.Status == status);
            }

            // If check-in and check-out dates are provided, filter out rooms with overlapping active bookings
            if (checkIn.HasValue && checkOut.HasValue && checkOut.Value > checkIn.Value)
            {
                var busyRoomIds = await _context.Bookings
                    .Where(b => b.BookingStatus != "Cancelled" &&
                                checkIn.Value.Date < b.CheckOutDate.Date &&
                                checkOut.Value.Date > b.CheckInDate.Date)
                    .Select(b => b.RoomId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(r => !busyRoomIds.Contains(r.RoomId));
            }

            var rooms = await query
                .OrderBy(r => r.FloorNumber)
                .ThenBy(r => r.RoomNumber)
                .ToListAsync();

            ViewBag.RoomTypes = await _context.RoomTypes.OrderBy(rt => rt.TypeName).ToListAsync();
            ViewBag.SelectedTypeId = typeId;
            ViewBag.SelectedMaxPrice = maxPrice;
            ViewBag.SelectedGuests = guests;
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;

            return View(rooms);
        }

        // GET: /Rooms/Manage (Admin Room Table View)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var rooms = await _context.Rooms
                .Include(r => r.RoomType)
                .OrderBy(r => r.FloorNumber)
                .ThenBy(r => r.RoomNumber)
                .ToListAsync();

            return View(rooms);
        }

        // GET: /Rooms/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: /Rooms/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.RoomTypes = await _context.RoomTypes.OrderBy(rt => rt.TypeName).ToListAsync();
            return View();
        }

        // POST: /Rooms/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            bool roomNumberExists = await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber);
            if (roomNumberExists)
            {
                ModelState.AddModelError("RoomNumber", "A room with this room number already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Room {room.RoomNumber} created successfully.";
                return RedirectToAction(nameof(Manage));
            }

            ViewBag.RoomTypes = await _context.RoomTypes.OrderBy(rt => rt.TypeName).ToListAsync();
            return View(room);
        }

        // GET: /Rooms/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            ViewBag.RoomTypes = await _context.RoomTypes.OrderBy(rt => rt.TypeName).ToListAsync();
            return View(room);
        }

        // POST: /Rooms/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            bool roomNumberConflict = await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber && r.RoomId != id);
            if (roomNumberConflict)
            {
                ModelState.AddModelError("RoomNumber", "Another room with this room number already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Room {room.RoomNumber} updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.RoomId))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Manage));
            }

            ViewBag.RoomTypes = await _context.RoomTypes.OrderBy(rt => rt.TypeName).ToListAsync();
            return View(room);
        }

        // GET: /Rooms/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: /Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room != null)
            {
                if (room.Bookings.Any(b => b.BookingStatus != "Cancelled" && b.CheckOutDate >= DateTime.Today))
                {
                    TempData["ErrorMessage"] = $"Cannot delete Room {room.RoomNumber} because it has active or upcoming reservations.";
                    return RedirectToAction(nameof(Manage));
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Room {room.RoomNumber} was successfully removed.";
            }

            return RedirectToAction(nameof(Manage));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }
    }
}
