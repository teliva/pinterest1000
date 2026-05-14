namespace PinterestApi.Models;

public class ImageCategory
{
    public int CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Image> Images { get; set; } = new List<Image>();
}