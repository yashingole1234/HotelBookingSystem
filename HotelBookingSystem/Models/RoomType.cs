using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models
{
    public class RoomType
    {
        public int RoomTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string TypeName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 100000)]
        public decimal PricePerNight { get; set; }

        [Range(1, 20)]
        public int Capacity { get; set; }

        public ICollection<Room> Rooms { get; set; }
            = new List<Room>();
    }
}