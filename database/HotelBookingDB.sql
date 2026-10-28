-- ============================================================================
-- HOTEL ROOM BOOKING SYSTEM (THE GRAND LUXE HOTEL & RESORTS)
-- SQL Server Database Schema & Sample Data Script
-- Database Name: HotelBookingDB
-- ============================================================================

USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HotelBookingDB')
BEGIN
    CREATE DATABASE [HotelBookingDB];
END
GO

USE [HotelBookingDB];
GO

-- ============================================================================
-- 1. DROP EXISTING CONSTRAINTS & TABLES IF RE-CREATING
-- ============================================================================
IF OBJECT_ID(N'dbo.Bookings', N'U') IS NOT NULL DROP TABLE [dbo].[Bookings];
IF OBJECT_ID(N'dbo.Rooms', N'U') IS NOT NULL DROP TABLE [dbo].[Rooms];
IF OBJECT_ID(N'dbo.RoomTypes', N'U') IS NOT NULL DROP TABLE [dbo].[RoomTypes];
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE [dbo].[Users];
GO

-- ============================================================================
-- 2. CREATE TABLE: Users
-- ============================================================================
CREATE TABLE [dbo].[Users] (
    [UserId] INT IDENTITY(1,1) NOT NULL,
    [FullName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(150) NOT NULL,
    [PasswordHash] NVARCHAR(255) NOT NULL,
    [Phone] NVARCHAR(15) NULL,
    [Role] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Users_Role] DEFAULT ('Guest'),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Users_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [UQ_Users_Email] UNIQUE NONCLUSTERED ([Email] ASC),
    CONSTRAINT [CK_Users_Role] CHECK ([Role] IN ('Admin', 'Guest'))
);
GO

-- ============================================================================
-- 3. CREATE TABLE: RoomTypes
-- ============================================================================
CREATE TABLE [dbo].[RoomTypes] (
    [RoomTypeId] INT IDENTITY(1,1) NOT NULL,
    [TypeName] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [PricePerNight] DECIMAL(10,2) NOT NULL,
    [Capacity] INT NOT NULL,
    CONSTRAINT [PK_RoomTypes] PRIMARY KEY CLUSTERED ([RoomTypeId] ASC),
    CONSTRAINT [CK_RoomTypes_Price] CHECK ([PricePerNight] > 0),
    CONSTRAINT [CK_RoomTypes_Capacity] CHECK ([Capacity] > 0)
);
GO

-- ============================================================================
-- 4. CREATE TABLE: Rooms
-- ============================================================================
CREATE TABLE [dbo].[Rooms] (
    [RoomId] INT IDENTITY(1,1) NOT NULL,
    [RoomNumber] NVARCHAR(20) NOT NULL,
    [RoomTypeId] INT NOT NULL,
    [FloorNumber] INT NOT NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Rooms_Status] DEFAULT ('Available'),
    [Description] NVARCHAR(500) NULL,
    [ImageUrl] NVARCHAR(500) NULL,
    CONSTRAINT [PK_Rooms] PRIMARY KEY CLUSTERED ([RoomId] ASC),
    CONSTRAINT [UQ_Rooms_RoomNumber] UNIQUE NONCLUSTERED ([RoomNumber] ASC),
    CONSTRAINT [FK_Rooms_RoomTypes] FOREIGN KEY ([RoomTypeId]) REFERENCES [dbo].[RoomTypes] ([RoomTypeId]) ON DELETE NO ACTION,
    CONSTRAINT [CK_Rooms_Status] CHECK ([Status] IN ('Available', 'Maintenance', 'Booked', 'Occupied'))
);
GO

-- ============================================================================
-- 5. CREATE TABLE: Bookings
-- ============================================================================
CREATE TABLE [dbo].[Bookings] (
    [BookingId] INT IDENTITY(1,1) NOT NULL,
    [UserId] INT NOT NULL,
    [RoomId] INT NOT NULL,
    [CheckInDate] DATE NOT NULL,
    [CheckOutDate] DATE NOT NULL,
    [NumberOfGuests] INT NOT NULL,
    [TotalAmount] DECIMAL(10,2) NOT NULL,
    [BookingStatus] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Bookings_Status] DEFAULT ('Pending'),
    [BookingDate] DATETIME2(7) NOT NULL CONSTRAINT [DF_Bookings_BookingDate] DEFAULT (SYSUTCDATETIME()),
    [SpecialRequest] NVARCHAR(500) NULL,
    CONSTRAINT [PK_Bookings] PRIMARY KEY CLUSTERED ([BookingId] ASC),
    CONSTRAINT [FK_Bookings_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Bookings_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms] ([RoomId]) ON DELETE NO ACTION,
    CONSTRAINT [CK_Bookings_Dates] CHECK ([CheckOutDate] > [CheckInDate]),
    CONSTRAINT [CK_Bookings_Guests] CHECK ([NumberOfGuests] > 0),
    CONSTRAINT [CK_Bookings_Amount] CHECK ([TotalAmount] >= 0),
    CONSTRAINT [CK_Bookings_Status] CHECK ([BookingStatus] IN ('Pending', 'Confirmed', 'CheckedIn', 'CheckedOut', 'Completed', 'Cancelled'))
);
GO

-- ============================================================================
-- 6. PERFORMANCE INDEXES
-- ============================================================================
CREATE NONCLUSTERED INDEX [IX_Bookings_UserId] ON [dbo].[Bookings] ([UserId] ASC);
CREATE NONCLUSTERED INDEX [IX_Bookings_RoomId] ON [dbo].[Bookings] ([RoomId] ASC);
CREATE NONCLUSTERED INDEX [IX_Bookings_Dates] ON [dbo].[Bookings] ([CheckInDate] ASC, [CheckOutDate] ASC);
CREATE NONCLUSTERED INDEX [IX_Rooms_RoomTypeId] ON [dbo].[Rooms] ([RoomTypeId] ASC);
GO

-- ============================================================================
-- 7. SEED INITIAL USERS
-- Admin Password:  Admin@123
-- Guest Password:  123456
-- Hashes generated via ASP.NET Core PasswordHasher<User> (PBKDF2 HMAC-SHA256)
-- ============================================================================
SET IDENTITY_INSERT [dbo].[Users] ON;

INSERT INTO [dbo].[Users] ([UserId], [FullName], [Email], [PasswordHash], [Phone], [Role], [CreatedAt])
VALUES
(1, N'Hotel Administrator', N'admin@grandluxe.com', N'AQAAAAIAAYagAAAAEB5qC2R5u2n6Vf6yXzUe1N+l4xZ+dO2a6g3lZ+B7X3c8m1q4w==', N'+91 9876543210', N'Admin', '2026-08-01 10:00:00'),
(2, N'Yash Ingole', N'yash@gmail.com', N'AQAAAAIAAYagAAAAEPwc6dLlzgUT59AhJBjCExpqLpj1+6erBGrkeAtgrIMpyqenM8jZjjvlyVXcmGBjcw==', N'+91 9876543210', N'Guest', '2026-09-15 14:30:00'),
(3, N'Priya Sharma', N'priya.sharma@example.com', N'AQAAAAIAAYagAAAAEPwc6dLlzgUT59AhJBjCExpqLpj1+6erBGrkeAtgrIMpyqenM8jZjjvlyVXcmGBjcw==', N'+91 9822334455', N'Guest', '2026-10-01 09:15:00');

SET IDENTITY_INSERT [dbo].[Users] OFF;
GO

-- ============================================================================
-- 8. SEED ROOM CATEGORIES (RoomTypes)
-- ============================================================================
SET IDENTITY_INSERT [dbo].[RoomTypes] ON;

INSERT INTO [dbo].[RoomTypes] ([RoomTypeId], [TypeName], [Description], [PricePerNight], [Capacity])
VALUES
(1, N'Standard Comfort Queen', N'Cozy retreat featuring a plush queen-sized bed, high-speed Wi-Fi, work desk, and ensuite marble shower.', 2500.00, 2),
(2, N'Deluxe King Room', N'Spacious luxury room with an ultra-comfort king bed, city skyline view, private balcony, and soaking bathtub.', 4200.00, 3),
(3, N'Executive Business Suite', N'Sophisticated suite with separate living area, ergonomic workstation, espresso machine, and club lounge access.', 6800.00, 4),
(4, N'Oceanfront Panoramic Villa', N'Ultimate seaside luxury with private terrace, heated jacuzzi, floor-to-ceiling panoramic windows, and 24/7 butler service.', 12500.00, 5),
(5, N'Royal Presidential Penthouse', N'The pinnacle of opulence: multi-room top-floor suite with private dining, wrap-around balcony, and VIP luxury amenities.', 22000.00, 6);

SET IDENTITY_INSERT [dbo].[RoomTypes] OFF;
GO

-- ============================================================================
-- 9. SEED HOTEL ROOMS
-- ============================================================================
SET IDENTITY_INSERT [dbo].[Rooms] ON;

INSERT INTO [dbo].[Rooms] ([RoomId], [RoomNumber], [RoomTypeId], [FloorNumber], [Status], [Description], [ImageUrl])
VALUES
(1, N'101', 1, 1, N'Available', N'Serene ground-floor queen room overlooking the tranquil courtyard gardens.', N'/images/rooms/room-101.jpg'),
(2, N'102', 1, 1, N'Available', N'Comfortable standard queen room with ambient lighting, smart TV, and premium linen.', N'/images/rooms/room-102.jpg'),
(3, N'201', 2, 2, N'Available', N'Elegant deluxe room with sunset balcony, king bed, and complimentary breakfast.', N'/images/rooms/room-201.jpg'),
(4, N'202', 2, 2, N'Available', N'Modern deluxe suite with marble bathroom, high-speed Wi-Fi, and rainfall shower.', N'/images/rooms/room-202.jpg'),
(5, N'301', 3, 3, N'Available', N'Spacious corner executive suite featuring lounge sofa, workstation, and espresso bar.', N'/images/rooms/room-301.jpg'),
(6, N'302', 3, 3, N'Available', N'Refined business suite with meeting corner, fast fiber internet, and panoramic views.', N'/images/rooms/room-302.jpg'),
(7, N'401', 4, 4, N'Available', N'Luxury seaside villa experience with private deck, open jacuzzi, and custom minibar.', N'/images/rooms/room-401.jpg'),
(8, N'501', 5, 5, N'Available', N'Exclusive presidential penthouse featuring grand living room, master bathroom with spa tub, and VIP terrace.', N'/images/rooms/room-501.jpg');

SET IDENTITY_INSERT [dbo].[Rooms] OFF;
GO

-- ============================================================================
-- 10. SEED SAMPLE RESERVATIONS (Bookings)
-- ============================================================================
SET IDENTITY_INSERT [dbo].[Bookings] ON;

INSERT INTO [dbo].[Bookings] ([BookingId], [UserId], [RoomId], [CheckInDate], [CheckOutDate], [NumberOfGuests], [TotalAmount], [BookingStatus], [BookingDate], [SpecialRequest])
VALUES
(1, 2, 1, '2026-10-10', '2026-10-13', 2, 7500.00, N'CheckedOut', '2026-10-01 11:20:00', N'Quiet room on lower floor requested.'),
(2, 2, 3, '2026-10-26', '2026-10-29', 2, 12600.00, N'CheckedIn', '2026-10-24 16:45:00', N'Late check-in around 8 PM.'),
(3, 2, 5, '2026-11-05', '2026-11-08', 3, 20400.00, N'Confirmed', '2026-10-27 18:10:00', N'Honeymoon setup with flowers and chocolates.'),
(4, 3, 4, '2026-11-12', '2026-11-15', 2, 12600.00, N'Pending', '2026-10-28 00:30:00', N'Airport pickup requested.');

SET IDENTITY_INSERT [dbo].[Bookings] OFF;
GO

PRINT N'HotelBookingDB schema & sample data created successfully!';
GO
