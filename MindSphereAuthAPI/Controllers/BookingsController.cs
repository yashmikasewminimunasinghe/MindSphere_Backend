using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindSphereAuthAPI.Data;
using MindSphereAuthAPI.Dtos;
using MindSphereAuthAPI.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MindSphereAuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Client,Counselor,Admin")]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/Bookings/availableCounsellors
        [HttpGet("availableCounsellors")]
        public async Task<IActionResult> GetAvailableCounsellorsWithSlots()
        {
            var counsellors = await _context.Counsellors.ToListAsync();
            var result = new List<object>();

            foreach (var counsellor in counsellors)
            {
                var allSlots = GenerateDummySlotsForCounsellor();

                var bookedSlots = await _context.Bookings
                    .Where(b => b.CounsellorId == counsellor.Id)
                    .Select(b => b.Slot)
                    .ToListAsync();

                var bookedNormalized = new HashSet<DateTime>(
                    bookedSlots.Select(dt => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0))
                );

                var availableSlots = allSlots
                    .Where(slot => !bookedNormalized.Contains(slot))
                    .ToList();

                var slotStrings = availableSlots.Select(s => s.ToString("yyyy-MM-dd HH:mm")).ToList();

                result.Add(new
                {
                    Id = counsellor.Id,
                    Name = counsellor.Name,
                    Specialty = counsellor.Specialty,
                    PhotoUrl = counsellor.PhotoUrl,
                    Rating = counsellor.Rating,
                    AvailableSlots = slotStrings
                });
            }

            return Ok(result);
        }

        // POST api/Bookings
        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> BookSession([FromBody] BookingCreateDto dto)
        {
            var userId = User.FindFirstValue("id");
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in token." });

            if (!DateTime.TryParseExact(dto.Slot, "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime incomingSlot))
            {
                return BadRequest(new { message = "Invalid slot datetime format." });
            }

            incomingSlot = new DateTime(
                incomingSlot.Year,
                incomingSlot.Month,
                incomingSlot.Day,
                incomingSlot.Hour,
                incomingSlot.Minute,
                0
            );

            bool exists = await _context.Bookings.AnyAsync(b =>
                b.CounsellorId == dto.CounsellorId &&
                b.Slot.Year == incomingSlot.Year &&
                b.Slot.Month == incomingSlot.Month &&
                b.Slot.Day == incomingSlot.Day &&
                b.Slot.Hour == incomingSlot.Hour &&
                b.Slot.Minute == incomingSlot.Minute
            );

            if (exists)
                return BadRequest(new { message = "This slot has already been booked." });

            var booking = new Booking
            {
                ClientId = userId,
                CounsellorId = dto.CounsellorId,
                Slot = incomingSlot,
                Notes = dto.Notes,
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }

        // GET api/Bookings/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Counsellor)
                .Include(b => b.Client)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        // GET api/Bookings/mine (for Counselor or Admin)
        [HttpGet("mine")]
        [Authorize(Roles = "Counselor,Admin")]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirstValue("id");
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in token." });

            var counsellor = await _context.Counsellors
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (counsellor == null)
                return NotFound(new { message = $"Counsellor not found for current user - {userId}." });

            var bookings = await _context.Bookings
                .Where(b => b.CounsellorId == counsellor.Id)
                .Include(b => b.Client)
                .ToListAsync();

            return Ok(bookings);
        }

        // ✅ NEW: GET api/Bookings/client (for Client)
        [HttpGet("client")]
        [Authorize]
        public async Task<IActionResult> GetClientBookings()
        {
            var userId = User.FindFirstValue("id");
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in token." });

            var bookings = await _context.Bookings
                .Where(b => b.ClientId == userId)
                .Include(b => b.Counsellor)
                .ToListAsync();

            return Ok(bookings);
        }

        // Dummy slot generator — replace with real availability logic
        private List<DateTime> GenerateDummySlotsForCounsellor()
        {
            var slots = new List<DateTime>();
            DateTime today = DateTime.Today;

            for (int dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                DateTime day = today.AddDays(dayOffset);
                slots.Add(new DateTime(day.Year, day.Month, day.Day, 9, 0, 0));
                slots.Add(new DateTime(day.Year, day.Month, day.Day, 10, 0, 0));
                slots.Add(new DateTime(day.Year, day.Month, day.Day, 11, 0, 0));
            }

            return slots;
        }


        // POST: api/Bookings/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingDto cancelDto)
        {
            var userId = User.FindFirstValue("id");
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Counsellor)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound(new { message = "Booking not found." });

            if (booking.IsCanceled)
                return BadRequest(new { message = "Booking is already canceled." });

            if (userRole == "Client" && booking.ClientId != userId)
                return Forbid("You are not allowed to cancel this booking.");

            if (userRole == "Counselor")
            {
                var counselor = await _context.Counsellors.FirstOrDefaultAsync(c => c.UserId == userId);
                if (counselor == null || counselor.Id != booking.CounsellorId)
                    return Forbid("You are not allowed to cancel this booking.");
            }

            booking.IsCanceled = true;
            booking.CanceledBy = userRole;
            booking.CancelReason = cancelDto.Reason;
            booking.CanceledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // TODO: Send email/in-app notification here

            return Ok(new { message = "Booking canceled successfully." });
        }

    }
}
