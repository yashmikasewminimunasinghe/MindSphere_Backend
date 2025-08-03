using System.Collections.Generic;

namespace MindSphereAuthAPI.DTOs
{
    public class CounsellorReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialty { get; set; }
        public double Rating { get; set; }
        public string PhotoUrl { get; set; }
        public List<string> AvailableSlots { get; set; }
    }
}
