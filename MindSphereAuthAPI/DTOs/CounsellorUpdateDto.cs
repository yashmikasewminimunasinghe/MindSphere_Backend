using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MindSphereAuthAPI.DTOs
{
    public class CounsellorUpdateDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Specialty { get; set; }

        public List<string> AvailableSlots { get; set; }
    }
}
