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
        PythonKeyWordsResponse? embedding = null;
        if (!string.IsNullOrWhiteSpace(req.text))
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("http://python_api:8000/keywords", new { text = req.text });

            if (!response.IsSuccessStatusCode)
                return Problem("Failed to generate embedding from Python API.");

            var result = await response.Content.ReadFromJsonAsync<PythonKeyWordsResponse>();
            embedding = result?.embedding;
        }

        double? categoryScore = null, roomTypeScore = null, styleScore = null;

        if (embedding != null)
        {
            var vectorJson = $"[{string.Join(",", embedding)}]";

            if (!categoryId.HasValue)
            {
                var match = await db.Database
                    .SqlQuery<SimilarityMatch>($"""
                        SELECT TOP 1 category_id AS Id, VECTOR_DISTANCE('cosine', embedding, CAST({vectorJson} AS VECTOR(384))) AS Score
                        FROM image_categories
                        WHERE embedding IS NOT NULL
                        ORDER BY Score
                        """)
                    .FirstOrDefaultAsync();
                
                categoryScore = match?.Score;
                if (match?.Score < 0.4) categoryId = match?.Id;
            }

            if (!roomTypeId.HasValue)
            {
                var match = await db.Database
                    .SqlQuery<SimilarityMatch>($"""
                        SELECT TOP 1 room_type_id AS Id, VECTOR_DISTANCE('cosine', embedding, CAST({vectorJson} AS VECTOR(384))) AS Score
                        FROM image_room_type
                        WHERE embedding IS NOT NULL
                        ORDER BY Score
                        """)
                    .FirstOrDefaultAsync();
                    roomTypeScore = match?.Score;
                    if (match?.Score < 0.4) roomTypeId = match?.Id;
            }

            if (!styleId.HasValue)
            {
                var match = await db.Database
                    .SqlQuery<SimilarityMatch>($"""
                        SELECT TOP 1 style_id AS Id, VECTOR_DISTANCE('cosine', embedding, CAST({vectorJson} AS VECTOR(384))) AS Score
                        FROM image_style
                        WHERE embedding IS NOT NULL
                        ORDER BY Score
                        """)
                    .FirstOrDefaultAsync();
                styleScore = match?.Score;
                if (match?.Score < 0.4) styleId = match?.Id;
            }
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

        var images = await query.ToListAsync();
        return Ok(new SearchResponse(images, req.text, categoryId, categoryScore, roomTypeId, roomTypeScore, styleId, styleScore));
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
