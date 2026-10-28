using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public int UserId { get; set; }

        public int RoomId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        [Range(0, 10000000)]
        public decimal TotalAmount { get; set; }

        [Required]
        public string BookingStatus { get; set; } = "Pending";

        public DateTime BookingDate { get; set; }

        [StringLength(500)]
        public string? SpecialRequest { get; set; }

        public User? User { get; set; }

        public Room? Room { get; set; }
    }
}