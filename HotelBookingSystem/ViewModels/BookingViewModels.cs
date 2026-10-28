using System.ComponentModel.DataAnnotations;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
    public class CreateBookingViewModel
    {
        public int RoomId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public string RoomTypeName { get; set; } = string.Empty;

        public decimal PricePerNight { get; set; }

        public int MaxCapacity { get; set; }

        public string? ImageUrl { get; set; }

        public string? RoomDescription { get; set; }

        [Required(ErrorMessage = "Please select a check-in date")]
        [DataType(DataType.Date)]
        [Display(Name = "Check-In Date")]
        public DateTime CheckInDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please select a check-out date")]
        [DataType(DataType.Date)]
        [Display(Name = "Check-Out Date")]
        public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Please specify number of guests")]
        [Range(1, 20, ErrorMessage = "Number of guests must be at least 1")]
        [Display(Name = "Number of Guests")]
        public int NumberOfGuests { get; set; } = 1;

        [StringLength(500, ErrorMessage = "Special requests cannot exceed 500 characters")]
        [Display(Name = "Special Requests (Optional)")]
        public string? SpecialRequest { get; set; }

        public int CalculatedNights => Math.Max(1, (CheckOutDate - CheckInDate).Days);

        public decimal EstimatedTotal => CalculatedNights * PricePerNight;
    }

    public class BookingConfirmationViewModel
    {
        public int BookingId { get; set; }
        public string ReferenceNumber => $"LUXE-{BookingId:D5}";
        public string GuestName { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string GuestPhone { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalAmount { get; set; }
        public string BookingStatus { get; set; } = "Confirmed";
        public DateTime BookingDate { get; set; }
        public string? SpecialRequest { get; set; }
        public string? ImageUrl { get; set; }
    }
}
