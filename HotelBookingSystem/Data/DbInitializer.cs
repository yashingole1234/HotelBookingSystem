using HotelBookingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            var passwordHasher = new PasswordHasher<User>();

            // 1. Seed / Ensure Users (Admin & Guest)
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@grandluxe.com");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    FullName = "Hotel Administrator",
                    Email = "admin@grandluxe.com",
                    Phone = "+91 9876543210",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");
                context.Users.Add(adminUser);
            }

            var guestUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "yash@gmail.com");
            if (guestUser == null)
            {
                guestUser = new User
                {
                    FullName = "Yash Ingole",
                    Email = "yash@gmail.com",
                    Phone = "+91 9876543210",
                    Role = "Guest",
                    CreatedAt = DateTime.UtcNow.AddMonths(-1)
                };
                guestUser.PasswordHash = passwordHasher.HashPassword(guestUser, "123456");
                context.Users.Add(guestUser);
            }

            await context.SaveChangesAsync();

            // 2. Seed / Ensure Room Types
            var roomTypesToEnsure = new List<RoomType>
            {
                new RoomType
                {
                    TypeName = "Standard Comfort Queen",
                    Description = "Cozy retreat featuring a plush queen-sized bed, high-speed Wi-Fi, work desk, and ensuite marble shower.",
                    PricePerNight = 2500.00m,
                    Capacity = 2
                },
                new RoomType
                {
                    TypeName = "Deluxe King Room",
                    Description = "Spacious luxury room with an ultra-comfort king bed, city skyline view, private balcony, and soaking bathtub.",
                    PricePerNight = 4200.00m,
                    Capacity = 3
                },
                new RoomType
                {
                    TypeName = "Executive Business Suite",
                    Description = "Sophisticated suite with separate living area, ergonomic workstation, espresso machine, and club lounge access.",
                    PricePerNight = 6800.00m,
                    Capacity = 4
                },
                new RoomType
                {
                    TypeName = "Oceanfront Panoramic Villa",
                    Description = "Ultimate seaside luxury with private terrace, heated jacuzzi, floor-to-ceiling panoramic windows, and 24/7 butler service.",
                    PricePerNight = 12500.00m,
                    Capacity = 5
                },
                new RoomType
                {
                    TypeName = "Royal Presidential Penthouse",
                    Description = "The pinnacle of opulence: multi-room top-floor suite with private dining, wrap-around balcony, and VIP luxury amenities.",
                    PricePerNight = 22000.00m,
                    Capacity = 6
                }
            };

            foreach (var rt in roomTypesToEnsure)
            {
                bool exists = await context.RoomTypes.AnyAsync(t => t.TypeName == rt.TypeName);
                if (!exists)
                {
                    context.RoomTypes.Add(rt);
                }
            }
            await context.SaveChangesAsync();

            // Fetch room type IDs for reference
            var stdType = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeName.Contains("Standard")) 
                          ?? await context.RoomTypes.FirstAsync();
            var dlxType = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeName.Contains("Deluxe")) 
                          ?? stdType;
            var exeType = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeName.Contains("Executive")) 
                          ?? dlxType;
            var vilType = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeName.Contains("Villa") || rt.TypeName.Contains("Ocean")) 
                          ?? exeType;
            var penType = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeName.Contains("Penthouse") || rt.TypeName.Contains("Presidential")) 
                          ?? exeType;

            // 3. Seed / Ensure Rooms
            var roomsToEnsure = new List<Room>
            {
                new Room
                {
                    RoomNumber = "101",
                    RoomTypeId = stdType.RoomTypeId,
                    FloorNumber = 1,
                    Status = "Available",
                    Description = "Serene ground-floor queen room overlooking the tranquil courtyard gardens.",
                    ImageUrl = "/images/rooms/room-101.jpg"
                },
                new Room
                {
                    RoomNumber = "102",
                    RoomTypeId = stdType.RoomTypeId,
                    FloorNumber = 1,
                    Status = "Available",
                    Description = "Comfortable standard queen room with ambient lighting, smart TV, and premium linen.",
                    ImageUrl = "/images/rooms/room-102.jpg"
                },
                new Room
                {
                    RoomNumber = "201",
                    RoomTypeId = dlxType.RoomTypeId,
                    FloorNumber = 2,
                    Status = "Available",
                    Description = "Elegant deluxe room with sunset balcony, king bed, and complimentary breakfast.",
                    ImageUrl = "/images/rooms/room-201.jpg"
                },
                new Room
                {
                    RoomNumber = "202",
                    RoomTypeId = dlxType.RoomTypeId,
                    FloorNumber = 2,
                    Status = "Available",
                    Description = "Modern deluxe suite with marble bathroom, high-speed Wi-Fi, and rainfall shower.",
                    ImageUrl = "/images/rooms/room-202.jpg"
                },
                new Room
                {
                    RoomNumber = "301",
                    RoomTypeId = exeType.RoomTypeId,
                    FloorNumber = 3,
                    Status = "Available",
                    Description = "Spacious corner executive suite featuring lounge sofa, workstation, and espresso bar.",
                    ImageUrl = "/images/rooms/room-301.jpg"
                },
                new Room
                {
                    RoomNumber = "302",
                    RoomTypeId = exeType.RoomTypeId,
                    FloorNumber = 3,
                    Status = "Available",
                    Description = "Refined business suite with meeting corner, fast fiber internet, and panoramic views.",
                    ImageUrl = "/images/rooms/room-302.jpg"
                },
                new Room
                {
                    RoomNumber = "401",
                    RoomTypeId = vilType.RoomTypeId,
                    FloorNumber = 4,
                    Status = "Available",
                    Description = "Luxury seaside villa experience with private deck, open jacuzzi, and custom minibar.",
                    ImageUrl = "/images/rooms/room-401.jpg"
                },
                new Room
                {
                    RoomNumber = "501",
                    RoomTypeId = penType.RoomTypeId,
                    FloorNumber = 5,
                    Status = "Available",
                    Description = "Exclusive presidential penthouse featuring grand living room, master bathroom with spa tub, and VIP terrace.",
                    ImageUrl = "/images/rooms/room-501.jpg"
                }
            };

            foreach (var r in roomsToEnsure)
            {
                var existingRoom = await context.Rooms.FirstOrDefaultAsync(x => x.RoomNumber == r.RoomNumber);
                if (existingRoom == null)
                {
                    context.Rooms.Add(r);
                }
                else
                {
                    // Enhance missing image or description on existing rooms
                    if (string.IsNullOrEmpty(existingRoom.ImageUrl))
                    {
                        existingRoom.ImageUrl = r.ImageUrl;
                    }
                    if (string.IsNullOrEmpty(existingRoom.Description))
                    {
                        existingRoom.Description = r.Description;
                    }
                }
            }
            await context.SaveChangesAsync();

            // 4. Seed Sample Bookings for Admin Analytics
            if (!await context.Bookings.AnyAsync())
            {
                var targetGuest = await context.Users.FirstOrDefaultAsync(u => u.Email == "yash@gmail.com") 
                                  ?? await context.Users.FirstOrDefaultAsync(u => u.Role == "Guest");
                var room101 = await context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.RoomNumber == "101");
                var room201 = await context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.RoomNumber == "201");
                var room301 = await context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.RoomNumber == "301");

                if (targetGuest != null && room101 != null && room201 != null && room301 != null)
                {
                    var pastBooking = new Booking
                    {
                        UserId = targetGuest.UserId,
                        RoomId = room101.RoomId,
                        CheckInDate = DateTime.Today.AddDays(-10),
                        CheckOutDate = DateTime.Today.AddDays(-7),
                        NumberOfGuests = 2,
                        TotalAmount = (room101.RoomType?.PricePerNight ?? 2500m) * 3,
                        BookingStatus = "CheckedOut",
                        BookingDate = DateTime.Today.AddDays(-15),
                        SpecialRequest = "Quiet room on lower floor requested."
                    };

                    var currentBooking = new Booking
                    {
                        UserId = targetGuest.UserId,
                        RoomId = room201.RoomId,
                        CheckInDate = DateTime.Today.AddDays(-1),
                        CheckOutDate = DateTime.Today.AddDays(2),
                        NumberOfGuests = 2,
                        TotalAmount = (room201.RoomType?.PricePerNight ?? 4200m) * 3,
                        BookingStatus = "CheckedIn",
                        BookingDate = DateTime.Today.AddDays(-3),
                        SpecialRequest = "Late check-in around 8 PM."
                    };

                    var upcomingBooking = new Booking
                    {
                        UserId = targetGuest.UserId,
                        RoomId = room301.RoomId,
                        CheckInDate = DateTime.Today.AddDays(5),
                        CheckOutDate = DateTime.Today.AddDays(8),
                        NumberOfGuests = 3,
                        TotalAmount = (room301.RoomType?.PricePerNight ?? 6800m) * 3,
                        BookingStatus = "Confirmed",
                        BookingDate = DateTime.Today.AddDays(-1),
                        SpecialRequest = "Honeymoon setup with flowers and chocolates."
                    };

                    await context.Bookings.AddRangeAsync(new[] { pastBooking, currentBooking, upcomingBooking });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
