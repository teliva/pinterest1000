using System.Text.Json;
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
    public async Task<IActionResult> Search(EmbeddingRequest req)
    {
        KeywordMatch[] keywords = [];
        if (!string.IsNullOrWhiteSpace(req.text))
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("http://localhost:8084/keywords", new { text = req.text });

            if (!response.IsSuccessStatusCode)
                return Problem("Failed to generate embedding from Python API.");

            var result = await response.Content.ReadFromJsonAsync<PythonKeyWordsResponse>();
            keywords = result?.keywords ?? [];
        }

        var categoryId = req.categoryId;
        var roomTypeId = req.roomTypeId;
        var styleId = req.styleId;
        double? categoryScore = null, roomTypeScore = null, styleScore = null;

        if (keywords.Length > 0)
        {
            var embeddingsJson = JsonSerializer.Serialize(keywords.Select(k => k.embedding));

            SpBestMatchResult? match = null;
            await db.Database.OpenConnectionAsync();
            try
            {
                var connection = (Microsoft.Data.SqlClient.SqlConnection)db.Database.GetDbConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.sp_FindBestMatches";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@embeddings_json", embeddingsJson);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int catIdOrd = reader.GetOrdinal("BestCategoryId"), catScoreOrd = reader.GetOrdinal("BestCategoryScore");
                    int roomIdOrd = reader.GetOrdinal("BestRoomTypeId"), roomScoreOrd = reader.GetOrdinal("BestRoomTypeScore");
                    int styleIdOrd = reader.GetOrdinal("BestStyleId"), styleScoreOrd = reader.GetOrdinal("BestStyleScore");
                    match = new SpBestMatchResult(
                        reader.IsDBNull(catIdOrd) ? null : reader.GetInt32(catIdOrd),
                        reader.IsDBNull(catScoreOrd) ? null : reader.GetDouble(catScoreOrd),
                        reader.IsDBNull(roomIdOrd) ? null : reader.GetInt32(roomIdOrd),
                        reader.IsDBNull(roomScoreOrd) ? null : reader.GetDouble(roomScoreOrd),
                        reader.IsDBNull(styleIdOrd) ? null : reader.GetInt32(styleIdOrd),
                        reader.IsDBNull(styleScoreOrd) ? null : reader.GetDouble(styleScoreOrd)
                    );
                }
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }

            if (match is not null)
            {
                if (!categoryId.HasValue && match.BestCategoryId is not null) { categoryScore = match.BestCategoryScore; if (match.BestCategoryScore < 0.4) categoryId = match.BestCategoryId; }
                if (!roomTypeId.HasValue && match.BestRoomTypeId is not null) { roomTypeScore = match.BestRoomTypeScore; if (match.BestRoomTypeScore < 0.4) roomTypeId = match.BestRoomTypeId; }
                if (!styleId.HasValue && match.BestStyleId is not null) { styleScore = match.BestStyleScore; if (match.BestStyleScore < 0.4 ) styleId = match.BestStyleId; }
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
        return Ok(new SearchResponse(images, req.text, categoryId, categoryScore, roomTypeId, roomTypeScore, styleId, styleScore, [.. keywords.Select(k => k.keyword)]));
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
