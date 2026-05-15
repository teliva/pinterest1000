using System.Text.Json.Serialization;

namespace PinterestApi.Models;

public class ImageCategory
{
    public int CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation property
    [JsonIgnore]
    public ICollection<Image> Images { get; set; } = new List<Image>();
}