using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(15)]
        public string? Phone { get; set; }

        [Required]
        public string Role { get; set; } = "Guest";

        public DateTime CreatedAt { get; set; }

        public ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();
    }
}