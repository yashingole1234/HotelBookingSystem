# The Grand Luxe Hotel & Suites — Hotel Room Booking System

> **ASP.NET Core 8.0 MVC Web Application & SQL Server**  
> Developed for **Pure Technology** .NET Intern Project Task.

---

## 1. Project Summary

**The Grand Luxe Hotel & Suites** is an enterprise-grade web application built with **ASP.NET Core 8.0 MVC**, **C#**, and **Microsoft SQL Server**. It provides a dual-portal hospitality management experience:

1. **Guest Portal:** Allows prospective guests to discover luxury suites and villas, filter by dates, capacity, and pricing, calculate real-time rates with hospitality taxes, place instant reservations with strict double-booking protection, and manage their stays via "My Reservations".
2. **Staff / Operations Portal:** Provides hotel administrators with an executive operations dashboard displaying live hotel KPIs (revenue, bookings, occupancy rate, room availability), room inventory management (CRUD), room category configuration, and front-desk reservation status updates (Confirm, Check-In, Check-Out, Cancel).

---

## 2. Technology Stack

- **Framework:** ASP.NET Core 8.0 MVC (Model-View-Controller)
- **Language:** C# 12
- **Database:** Microsoft SQL Server (SQLEXPRESS / LocalDB)
- **ORM / Data Access:** Entity Framework Core 7.0 / 8.0 with LINQ & Migrations
- **Authentication:** ASP.NET Core Cookie Authentication with Sliding Expiration
- **Security & Cryptography:** PBKDF2 HMAC-SHA256 Password Hashing via `Microsoft.Extensions.Identity.Core`
- **Frontend / Styling:** Custom Luxury Design System (`luxury-hotel.css`), Bootstrap 5.3, Bootstrap Icons CDN, Google Fonts (*Playfair Display* & *Plus Jakarta Sans*)
- **Animations:** CSS3 Keyframe animations (`fadeInUp`, `shimmer`, `livePulse`, hover transforms)

---

## 3. Directory Layout (Per Submission Specification)

```text
HotelBookingSystem/
├── HotelBookingSystem.sln             # Visual Studio Solution File
├── HotelBookingSystem/                # ASP.NET Core MVC Web Project (src)
│   ├── Controllers/                  # Account, Admin, Bookings, Home, Rooms
│   ├── Data/                         # ApplicationDbContext, DbInitializer (Seeder)
│   ├── Models/                       # User, Room, RoomType, Booking
│   ├── ViewModels/                   # AccountViewModels, BookingViewModels, AdminViewModels
│   ├── Views/                        # Account, Admin, Bookings, Home, Rooms, Shared
│   └── wwwroot/                      # CSS (luxury-hotel.css, site.css), JS, Libraries
├── database/
│   ├── HotelBookingDB.bak            # Native SQL Server Database Backup
│   ├── HotelBookingDB.sql            # Complete SQL Script (Schema + Sample Data)
│   └── ER-Diagram.png                # High-Resolution Database Entity Relationship Diagram
├── screenshots/                      # Desktop and mobile screenshots
│   └── README.md                     # Screenshot index and capture guidelines
├── AI_TOOLS_USED.md                  # Comprehensive AI usage, prompts, and developer review
└── README.md                         # Project documentation and design decisions
```

---

## 4. Quick Start & Setup Guide

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (with ASP.NET and web development workload) or Visual Studio Code
- Microsoft SQL Server 2019+ or SQL Server Express

### Step 1: Clone / Open Solution
Open `HotelBookingSystem.sln` in Visual Studio 2022.

### Step 2: Database Setup (Two Easy Options)
#### Option A (Automated Seeding — Recommended):
The project automatically runs `DbInitializer.SeedAsync()` on startup. Just verify the connection string in `HotelBookingSystem/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=HotelBookingDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

#### Option B (Using SQL Script or Backup):
- **Script:** Execute `database/HotelBookingDB.sql` in SQL Server Management Studio (SSMS) or Azure Data Studio.
- **Backup:** Restore `database/HotelBookingDB.bak` onto your SQL Server instance.

### Step 3: Run the Application
In Visual Studio, press **F5** (or run `dotnet run` in the `HotelBookingSystem` folder).  
The app will launch at `https://localhost:7xxx` or `http://localhost:5298`.

---

## 5. Test User Credentials

The database comes pre-seeded with both Administrator and Guest test accounts:

| Role | Email Address | Password | Permissions & Access |
| :--- | :--- | :--- | :--- |
| **Hotel Admin** | `admin@grandluxe.com` | `Admin@123` | Executive Dashboard, Manage Bookings, Room CRUD, Room Categories, Guest Accounts |
| **Guest Member** | `yash@gmail.com` | `123456` | Search Rooms, Book Suites, View My Reservations, Cancel Bookings, Profile |
| **Guest Member (Alt)** | `priya.sharma@example.com` | `123456` | Standard Guest privileges |

---

## 6. Design Decisions Section *(Mandated by Project Task)*

### 6.1 Features Chosen and Why
1. **Role-Based Cookie Authentication:**
   - Instead of heavyweight ASP.NET Core Identity with bloated boilerplate tables, we engineered **Cookie Authentication** coupled with `PasswordHasher<User>`. This fulfills the task's criteria for clean architecture, lightweight database footprint, and complete code ownership.
2. **Double-Booking & Overlapping Prevention:**
   - In a real hotel, allowing two guests to book the same room on overlapping dates is disastrous. We enforced a strict interval intersection check:
     $$\text{CheckIn} < \text{Existing.CheckOutDate} \quad \land \quad \text{CheckOut} > \text{Existing.CheckInDate}$$
     Cancelled bookings are ignored, ensuring released dates become instantly re-bookable.
3. **Dynamic Front-Desk Operations Workflow:**
   - Reservations flow through a complete lifecycle:  
     $$\text{Pending} \longrightarrow \text{Confirmed} \longrightarrow \text{CheckedIn} \longrightarrow \text{CheckedOut / Completed}$$
     When a guest checks in, the system automatically transitions the room status to `Booked` / `Occupied`; when they check out or cancel, the room returns to `Available`.
4. **Interactive Rate & Tax Calculator:**
   - Real-time client-side calculation calculates nights, subtotal, and 12% GST/Luxury Cess dynamically before submission, backed by server-side verification.
5. **Printable Confirmation Voucher:**
   - A digital voucher with confirmation code (`#LUXE-00042`), stay breakdown, and print styles (`@media print`) simulates real hospitality check-in passes.

### 6.2 Business Rules Applied
- **Date Constraints:** Check-in date cannot be in the past. Check-out date must be strictly after the check-in date. Minimum stay is 1 night.
- **Capacity Limits:** Number of guests cannot exceed `RoomType.Capacity`.
- **Guest Isolation:** Guests can only view and cancel their own bookings; administrators have global access.
- **Cancellation Grace Period:** Guests can only cancel reservations that have not yet commenced (`CheckInDate >= Today`).
- **Deletion Safety:** Rooms and Room Categories cannot be deleted if they have active reservations or assigned rooms.
- **CSRF & Anti-Forgery:** All state modifications are protected by cryptographically signed anti-forgery tokens.

### 6.3 What Would Be Added With More Time
1. **Payment Gateway Integration:** Stripe / Razorpay webhook integration for credit card authorization, capture, and instant refunds upon cancellation.
2. **Automated Transactional Emails:** SendGrid or SMTP dispatching PDF booking vouchers and check-in reminders 24 hours prior to arrival.
3. **Multi-Room Cart Booking:** Capability for corporate travel managers or families to reserve multiple rooms in a single transaction.
4. **Guest Reviews & Star Ratings:** Verified post-checkout review and photo submission system for guests.

---

## 7. Features Implemented Checklist

### Guest Features
- [x] Responsive Luxury Landing page with Hero search and amenity highlights
- [x] Room catalog with multi-parameter filtering (Category, Max Price, Min Guests, Availability)
- [x] Room detail view with amenities, specifications, high-res photos, and booking CTA
- [x] Date pickers with past date blocking and dynamic nights calculation
- [x] Strict overlap conflict prevention (prevents double-booking)
- [x] Booking confirmation receipt voucher with printable layout
- [x] "My Reservations" portal with tabbed filtering (All, Upcoming, Past, Cancelled)
- [x] Self-service booking cancellation with modal verification
- [x] Guest profile management with lifetime stay metrics

### Staff / Administrator Features
- [x] Executive Operations Dashboard with live KPIs: Total Revenue, Occupancy Rate, Total Bookings, Available Rooms
- [x] Recent reservations table with instant status transition dropdowns
- [x] Comprehensive Booking Management screen with search and status filtering
- [x] Front-Desk actions: Confirm, Check-In, Check-Out, and Cancel reservations
- [x] Automatic Room status synchronization (Available ↔ Booked ↔ Maintenance)
- [x] Full Room Inventory CRUD (Create, Read, Update, Delete) with image URLs and floor numbers
- [x] Room Category CRUD with custom pricing and capacity adjustments
- [x] Guest accounts register with role visibility and lifetime reservation counts

### Security & Quality
- [x] ASP.NET Core Cookie Authentication with 8-hour sliding expiration
- [x] PBKDF2 HMAC-SHA256 password hashing (no plain-text passwords stored)
- [x] Role-Based Access Control (`[Authorize(Roles = "Admin")]`, `[Authorize]`, `[AllowAnonymous]`)
- [x] Anti-Forgery token validation on all POST actions
- [x] Clean architecture with ViewModels, Data Annotations, and 0 compiler warnings
- [x] Database referential integrity, unique constraints, and foreign key indexes

---

## 8. Known Issues & Troubleshooting

- **Database Connection:** If SQL Server Express is installed with a different instance name, update the `Data Source` in `appsettings.json` to match your local server (e.g. `Server=(localdb)\\mssqllocaldb` or `Server=localhost`).
- **Port Assignment:** When running in Visual Studio, port numbers are assigned by `Properties/launchSettings.json`. Both HTTP and HTTPS profiles are supported.

---

## 9. Submission & Candidate Information

- **Candidate Name:** Yash Ingole
- **Email:** ingoleyash123@gmail.com
- **Phone:** +91 9876543210
- **GitHub Profile:** [github.com/yashingole1234](https://github.com/yashingole1234)
- **Repository:** [github.com/yashingole1234/HotelBookingSystem](https://github.com/yashingole1234/HotelBookingSystem)
- **Evaluator:** pranay.borode@puretechnology.in
- **HR Contact:** mayuri.a@puretechnology.in

