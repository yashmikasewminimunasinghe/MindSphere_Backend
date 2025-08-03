using System.ComponentModel.DataAnnotations;

namespace MindSphereAuthAPI.DTOs
{
    public class CounsellorCreateDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Specialty { get; set; }
        [Range(0, 5)]
        public double Rating { get; set; }
        public IFormFile Photo { get; set; } // Image file upload
        public List<string> AvailableSlots { get; set; }
    }
}
