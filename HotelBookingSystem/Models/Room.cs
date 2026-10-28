using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models
{
    public class Room
    {
        public int RoomId { get; set; }

        [Required]
        [StringLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        public int RoomTypeId { get; set; }

        public int FloorNumber { get; set; }

        [Required]
        public string Status { get; set; } = "Available";

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public RoomType? RoomType { get; set; }

        public ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();
    }
}