using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindSphereAuthAPI.Data;
using MindSphereAuthAPI.Dtos;
using MindSphereAuthAPI.Models;
using Stripe;
using Stripe.Checkout;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MindSphereAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public StripeController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] BookingCreateDto bookingDto)
        {
            if (bookingDto == null)
                return BadRequest("Booking data is required.");

            if (string.IsNullOrWhiteSpace(bookingDto.ClientId))
                return BadRequest("ClientId is required.");

            if (string.IsNullOrWhiteSpace(bookingDto.Slot))
                return BadRequest("Slot is required.");

            if (!DateTime.TryParse(bookingDto.Slot, out DateTime slotDateTime))
                return BadRequest("Invalid date format for Slot.");

            var domain = "http://localhost:3000";

            var booking = new Booking
            {
                ClientId = bookingDto.ClientId,
                CounsellorId = bookingDto.CounsellorId,
                Slot = slotDateTime,
                Notes = bookingDto.Notes,
                IsPaid = false,
                SessionLink = $"https://example.com/session/{Guid.NewGuid()}"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 10000,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Counseling Session"
                            },
                        },
                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{domain}/payment-success?bookingId={booking.Id}",
                CancelUrl = $"{domain}/payment-cancelled"
            };

            var service = new SessionService();
            Session session;

            try
            {
                session = await service.CreateAsync(options);
            }
            catch (StripeException ex)
            {
                return BadRequest($"Stripe error: {ex.Message}");
            }

            return Ok(new { id = session.Id });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var secret = _configuration["Stripe:WebhookSecret"];

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    secret
                );
            }
            catch (StripeException e)
            {
                return BadRequest($"Webhook error: {e.Message}");
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                var bookingIdParam = session.SuccessUrl?.Split("bookingId=").LastOrDefault();

                if (int.TryParse(bookingIdParam, out int bookingId))
                {
                    var booking = await _context.Bookings
                        .Include(b => b.Client)
                        .Include(b => b.Counsellor)
                        .FirstOrDefaultAsync(b => b.Id == bookingId);

                    if (booking != null && !booking.WebhookProcessed)
                    {
                        booking.IsPaid = true;
                        booking.WebhookProcessed = true;
                        await _context.SaveChangesAsync();

                        // Send emails
                        if (booking.Client?.Email != null)
                        {
                            await EmailService.SendSessionLinkAsync(
                                booking.Client.Email,
                                booking.SessionLink,
                                booking.Slot,
                                booking.Counsellor.Name,
                                booking.Client.FirstName
                            );
                        }

                        if (booking.Counsellor?.UserId != null)
                        {
                            var counsellorUser = await _context.Users
                                .FirstOrDefaultAsync(u => u.Id == booking.Counsellor.UserId);

                            if (counsellorUser?.Email != null)
                            {
                                await EmailService.SendSessionLinkAsync(
                                    counsellorUser.Email,
                                    booking.SessionLink,
                                    booking.Slot,
                                    booking.Counsellor.Name,
                                    booking.Counsellor.Name
                                );
                            }
                        }
                    }
                }
            }

            return Ok();
        }
    }
}
