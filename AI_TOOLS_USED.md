# AI Tools Used Report

This document reports all AI tools, models, prompts, and developer review/modifications used during the development of the **Hotel Room Booking System (The Grand Luxe Hotel & Suites)**, as mandated by the project requirements.

---

## 1. Overview Table of AI Tools

| Tool / Platform | Version / Plan | Purpose & Tasks Used For | Developer Review & Adjustments |
| :--- | :--- | :--- | :--- |
| **Google Gemini (Antigravity)** | Gemini 3.8 Flash (High) | Architectural design, ASP.NET Core Cookie Auth configuration, Controller & ViewModel scaffolding, CSS animation architecture, ER diagram generation script. | Verified entity relationships, corrected EF Core navigation nullability warnings (`CS8602`), adjusted SQL Server check constraints. |
| **ChatGPT (OpenAI)** | GPT-4o | SQL Server initial schema generation, table constraints, and sample data ideas. | Corrected column data types (changed `NVARCHAR` for dates to proper SQL `DATE`/`DATETIME2`), added performance indexes on foreign keys. |
| **GitHub Copilot** | Visual Studio Extension | Auto-completion for Razor markup, tag helpers, and bootstrap utility classes in views. | Refactored generated HTML into luxury design system classes (`btn-luxe-gold`, `room-card-luxe`), added client-side date math. |

---

## 2. Most Useful Prompts

Here are 4 of the most effective and structured prompts utilized during the project:

### Prompt 1: Cookie Authentication & PBKDF2 Password Hashing
> *"Configure ASP.NET Core 8.0 Cookie Authentication with `Microsoft.AspNetCore.Authentication.Cookies` and `Microsoft.Extensions.Identity.Core`'s `PasswordHasher<TUser>`. Include sliding expiration (8 hours), redirect paths for `/Account/Login` and `/Account/AccessDenied`, and provide `RegisterViewModel` and `LoginViewModel` with comprehensive data annotations."*

### Prompt 2: Overlapping Booking Prevention Business Rule
> *"Write an EF Core query in ASP.NET Core MVC to strictly prevent overlapping room reservations. If a user selects check-in date A and check-out date B for Room X, check whether any existing booking (excluding 'Cancelled') overlaps using the interval logic `checkIn < existing.CheckOutDate && checkOut > existing.CheckInDate`. Add a friendly error message to ModelState if conflict occurs."*

### Prompt 3: Luxury Hospitality Design System & Animations
> *"Design a luxury 5-star hotel CSS stylesheet with keyframe animations (`fadeInUp`, `shimmer`, `livePulse`), frosted glassmorphic navigation bar, luxury gold/midnight-navy color palette, interactive room cards with price tags and availability badges, and a live JavaScript price calculator for nights and taxes."*

### Prompt 4: Executive Admin Operations Dashboard
> *"Create an Admin Dashboard ViewModel and Razor view for hotel operations that aggregates Total Revenue (sum of non-cancelled bookings), Occupancy Rate (%), Total Bookings, Active Rooms, Pending Bookings, and displays a recent reservations table with immediate status transition dropdowns (Confirm, Check-In, Check-Out, Cancel). Protect it with `[Authorize(Roles = "Admin")]`."*

---

## 3. What Was Changed and Fixed by Developer

While AI tools provided scaffolding and syntax suggestions, significant manual logic, architectural decisions, and security hardening were engineered directly:

1. **Database Constraint Alignment:**
   - The initial AI suggestions did not account for existing SQL Server CHECK constraints (`CK_Bookings_Status` and `CK_Rooms_Status`). We resolved conflict errors by expanding allowed statuses to `('Pending', 'Confirmed', 'CheckedIn', 'CheckedOut', 'Completed', 'Cancelled')` and `('Available', 'Maintenance', 'Booked', 'Occupied')`.

2. **Nullability & Compiler Cleanliness (Zero Warnings):**
   - Fixed all EF Core `.ThenInclude()` nullable navigation warnings (`CS8602`) using safe null-forgiving operators (`r => r!.RoomType`), achieving a pristine build with **0 Warnings and 0 Errors**.

3. **Concurrency & Double-Booking Protection:**
   - AI code initially relied solely on client-side date pickers. We added server-side validation ensuring:
     - `CheckInDate >= DateTime.Today`
     - `CheckOutDate > CheckInDate`
     - `NumberOfGuests <= RoomType.Capacity`
     - Real-time SQL database overlap check before inserting any reservation record.

4. **Security & State Mutation:**
   - Enforced `[ValidateAntiForgeryToken]` and `@Html.AntiForgeryToken()` across all HTTP POST endpoints (`Create`, `Edit`, `Delete`, `Cancel`, `UpdateStatus`, `Logout`).
   - Ensured guest authorization isolation: guests can only view and cancel their own bookings; administrators have global access.

5. **Visual Identity & Luxury UX:**
   - Overhauled generic Bootstrap cards into a custom luxury theme featuring gold crest branding, responsive typography, live pulse indicators, and interactive night/tax calculators.
