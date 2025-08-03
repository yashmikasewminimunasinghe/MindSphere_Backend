// Models/ChangePasswordDto.cs
namespace MindSphereAuthAPI.Models
{
    public class ChangePasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}
