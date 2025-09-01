using Microsoft.AspNetCore.Identity;

namespace MindSphereAuthAPI.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string ContactNumber { get; set; }
        public bool IsFirstLogin { get; set; } = false;
    }
}
