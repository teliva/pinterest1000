using System.Text.Json.Serialization;

namespace PinterestApi.Models;

public class ImageRoomType
{
    public int RoomTypeId { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation property
    [JsonIgnore]
    public ICollection<Image> Images { get; set; } = new List<Image>();
}