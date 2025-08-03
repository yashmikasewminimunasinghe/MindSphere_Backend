namespace MindSphereAuthAPI.Dtos
{
    public class BookingCreateDto
    {
        
        public int CounsellorId { get; set; }
        public string Slot { get; set; }  // must be "yyyy-MM-dd HH:mm"
        public string Notes { get; set; }
        public string ClientId { get; set; }

    }
}
