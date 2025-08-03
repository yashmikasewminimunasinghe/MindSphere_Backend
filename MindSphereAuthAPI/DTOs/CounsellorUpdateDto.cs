using System.ComponentModel.DataAnnotations;

namespace MindSphereAuthAPI.DTOs
{
    public class CounsellorUpdateDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Specialty { get; set; }
        [Range(0, 5)]
        public double Rating { get; set; }
        public List<string> AvailableSlots { get; set; }
    }
}

