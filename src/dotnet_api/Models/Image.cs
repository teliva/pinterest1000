namespace PinterestApi.Models;

public class Image
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int CategoryId { get; set; }
    public int RoomTypeId { get; set; }
    public int StyleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ImageCategory? Category { get; set; }
    public ImageRoomType? RoomType { get; set; }
    public ImageStyle? Style { get; set; }
}