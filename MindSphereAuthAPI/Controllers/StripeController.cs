using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindSphereAuthAPI.Data;
using MindSphereAuthAPI.Dtos;
using MindSphereAuthAPI.Models;
using MindSphereAuthAPI.Services;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MindSphereAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public StripeController(IConfiguration configuration, ApplicationDbContext context, EmailService emailService)
        {
            _configuration = configuration;
            _context = context;
            _emailService = emailService;

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] BookingCreateDto bookingDto)
        {
            if (bookingDto == null)
                return BadRequest("Booking data is required.");

            if (string.IsNullOrWhiteSpace(bookingDto.ClientId))
                return BadRequest("ClientId is required.");

            if (!DateTime.TryParse(bookingDto.Slot, out DateTime slotDateTime))
                return BadRequest("Invalid date format for Slot.");

            // Generate Jitsi session link
            string sessionLink = "https://meet.jit.si/" + Guid.NewGuid();

            // Create booking in DB
            var booking = new Booking
            {
                ClientId = bookingDto.ClientId,
                CounsellorId = bookingDto.CounsellorId,
                Slot = slotDateTime,
                Notes = bookingDto.Notes,
                IsPaid = false,
                WebhookProcessed = false,
                SessionLink = sessionLink,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // --- Email sending ---
            var clientEmail = (await _context.Users.FindAsync(booking.ClientId))?.Email;
            var counselor = await _context.Counsellors.FirstOrDefaultAsync(c => c.Id == booking.CounsellorId);
            string counselorEmail = null;
            if (counselor != null)
            {
                counselorEmail = await _context.Users
                    .Where(u => u.Id == counselor.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
            }

            if (!string.IsNullOrEmpty(clientEmail) || !string.IsNullOrEmpty(counselorEmail))
            {
                string dateTime = booking.Slot.ToString("f"); // Example: Friday, 27 August 2025 10:00 AM
                await _emailService.SendSessionLinkAsync(clientEmail ?? "", counselorEmail ?? "", booking.SessionLink, dateTime);
            }
            // --- Email sending ends ---

            // --- Stripe payment setup ---
            var domain = _configuration["Stripe:FrontendUrl"];

            if (string.IsNullOrWhiteSpace(domain))
                return BadRequest(new { error = "FrontendUrl is not configured in appsettings.json" });

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 10000, // $100 in cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Counseling Session"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{domain}/payment-success?bookingId={booking.Id}",
                CancelUrl = $"{domain}/payment-cancelled",
                Metadata = new Dictionary<string, string>
                {
                    { "bookingId", booking.Id.ToString() }
                }
            };

            var service = new SessionService();
            Session session;
            try
            {
                session = await service.CreateAsync(options);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            booking.PaymentIntentId = session.PaymentIntentId;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                bookingId = booking.Id,
                sessionId = session.Id,
                sessionLink = booking.SessionLink
            });
        }

        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            var booking = await _context.Bookings
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            if (booking == null) return NotFound("No booking found to test email.");

            return Ok("Emails are sent after booking creation with Jitsi session link.");
        }
    }
}
