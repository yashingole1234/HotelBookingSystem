using System.Diagnostics;
using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var featuredRooms = await _context.Rooms
                .Include(r => r.RoomType)
                .Where(r => r.Status == "Available")
                .Take(4)
                .ToListAsync();

            var roomTypes = await _context.RoomTypes
                .OrderBy(rt => rt.PricePerNight)
                .ToListAsync();

            ViewBag.FeaturedRooms = featuredRooms;
            ViewBag.RoomTypes = roomTypes;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
