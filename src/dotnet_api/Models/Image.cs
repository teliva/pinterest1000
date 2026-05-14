using System.ComponentModel.DataAnnotations.Schema;

namespace PinterestApi.Models;

public class Image
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int CategoryId { get; set; }
    public int RoomTypeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public List<int>? StyleIds { get; set; }

    // Navigation properties
    public ImageCategory? Category { get; set; }
    public ImageRoomType? RoomType { get; set; }
    public ICollection<ImageStyle> Styles { get; set; } = new List<ImageStyle>();
}