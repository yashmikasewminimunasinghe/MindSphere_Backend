using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindSphereAuthAPI.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ClientId { get; set; }

        [ForeignKey("ClientId")]
        public AppUser Client { get; set; }

        [Required]
        public int CounsellorId { get; set; }

        [ForeignKey("CounsellorId")]
        public Counsellor Counsellor { get; set; }

        [Required]
        public DateTime Slot { get; set; }

        public string Notes { get; set; }
        public bool IsPaid { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // New Fields
        public bool IsCanceled { get; set; } = false;
        public string? CanceledBy { get; set; }  // Nullable
        public string? CancelReason { get; set; }  // Nullable
        public DateTime? CanceledAt { get; set; }  // Nullable

        public string? SessionLink { get; set; }

        public string? PaymentIntentId { get; set; }

        public bool WebhookProcessed { get; set; } = false;
    }
}
