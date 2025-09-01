using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Counsellor
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Specialty { get; set; }

    // Removed Rating and PhotoUrl from AddCounsellor form
    public double? Rating { get; set; }  // Optional now
    public string? PhotoUrl { get; set; } // Optional now

    public string UserId { get; set; }  // Add this!

    [Column("AvailableSlots")]
    [JsonIgnore]
    public string AvailableSlotsStorage { get; set; } = "";

    [NotMapped]
    [JsonPropertyName("availableSlots")]
    public List<string> AvailableSlots
    {
        get
        {
            if (string.IsNullOrWhiteSpace(AvailableSlotsStorage))
                return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(AvailableSlotsStorage) ?? new List<string>();
            }
            catch
            {
                return AvailableSlotsStorage.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            }
        }
        set
        {
            if (value == null || !value.Any())
                AvailableSlotsStorage = "[]";
            else
                AvailableSlotsStorage = JsonSerializer.Serialize(value);
        }
    }
}