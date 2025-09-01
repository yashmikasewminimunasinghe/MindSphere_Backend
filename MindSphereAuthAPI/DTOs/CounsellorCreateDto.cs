using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MindSphereAuthAPI.DTOs
{
    public class CounsellorCreateDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }  // <-- added

        [Required]
        public string Specialty { get; set; }

        public List<string> AvailableSlots { get; set; }
    }
}
