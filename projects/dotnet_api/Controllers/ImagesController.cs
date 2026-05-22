using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinterestApi.Data;
using PinterestApi.Models;

namespace PinterestApi.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController(ApplicationDbContext db, IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId, [FromQuery] int? roomTypeId, [FromQuery] int? styleId)
    {
        var query = db.Images.Include(i => i.Category)
                             .Include(i => i.RoomType)
                             .Include(i => i.Styles)
                             .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(i => i.CategoryId == categoryId.Value);

        if (roomTypeId.HasValue)
            query = query.Where(i => i.RoomTypeId == roomTypeId.Value);

        if (styleId.HasValue)
            query = query.Where(i => i.Styles.Any(s => s.StyleId == styleId.Value));

        return Ok(await query.ToListAsync());
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromQuery] int? categoryId, [FromQuery] int? roomTypeId, [FromQuery] int? styleId, EmbeddingRequest req)
    {
        float[]? embedding = null;
        if (req.text != null)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("http://python_api:8000/embeddings", new { text = req.text });

            if (!response.IsSuccessStatusCode)
                return Problem("Failed to generate embedding from Python API.");

            var result = await response.Content.ReadFromJsonAsync<PythonEmbeddingResponse>();
            embedding = result?.embedding;
        }

        if (embedding != null)
        {
            var vectorJson = $"[{string.Join(",", embedding)}]";

            if (!categoryId.HasValue)
                categoryId = await db.Database
                    .SqlQuery<int>($"""
                        SELECT TOP 1 category_id AS Value
                        FROM image_categories
                        WHERE embedding IS NOT NULL
                        ORDER BY VECTOR_DISTANCE('cosine', embedding, CAST({vectorJson} AS VECTOR(384)))
                        """)
                    .FirstOrDefaultAsync();

            if (!roomTypeId.HasValue)
                roomTypeId = await db.Database
                    .SqlQuery<int>($"""
                        SELECT TOP 1 room_type_id AS Value
                        FROM image_room_type
                        WHERE embedding IS NOT NULL
                        ORDER BY VECTOR_DISTANCE('cosine', embedding, CAST({vectorJson} AS VECTOR(384)))
                        """)
                    .FirstOrDefaultAsync();

            if (!styleId.HasValue)
                styleId = await db.Database
                    .SqlQuery<int>($"""
                        SELECT TOP 1 style_id AS Value
                        FROM image_style
                        WHERE embedding IS NOT NULL
                        ORDER BY VECTOR_DISTANCE('cosine', embedding, CAST({vectorJson} AS VECTOR(384)))
                        """)
                    .FirstOrDefaultAsync();
        }

        var query = db.Images.Include(i => i.Category)
                             .Include(i => i.RoomType)
                             .Include(i => i.Styles)
                             .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(i => i.CategoryId == categoryId.Value);

        if (roomTypeId.HasValue)
            query = query.Where(i => i.RoomTypeId == roomTypeId.Value);

        if (styleId.HasValue)
            query = query.Where(i => i.Styles.Any(s => s.StyleId == styleId.Value));

        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var image = await db.Images.Include(i => i.Category)
                                   .Include(i => i.RoomType)
                                   .Include(i => i.Styles)
                                   .FirstOrDefaultAsync(i => i.Id == id);
        return image is null ? NotFound() : Ok(image);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Image inputImage)
    {
        var image = await db.Images.Include(i => i.Styles).FirstOrDefaultAsync(i => i.Id == id);
        if (image is null) return NotFound();

        image.CategoryId = inputImage.CategoryId;
        image.RoomTypeId = inputImage.RoomTypeId;
        image.CreatedAt = inputImage.CreatedAt;

        if (inputImage.StyleIds is not null)
        {
            image.Styles.Clear();
            var styles = await db.ImageStyles
                .Where(s => inputImage.StyleIds.Contains(s.StyleId))
                .ToListAsync();
            foreach (var style in styles)
                image.Styles.Add(style);
        }

        await db.SaveChangesAsync();
        return NoContent();
    }
}
