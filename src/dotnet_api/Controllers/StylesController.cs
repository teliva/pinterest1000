using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinterestApi.Data;
using PinterestApi.Models;

namespace PinterestApi.Controllers;

[ApiController]
[Route("api/styles")]
public class StylesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.ImageStyles.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var style = await db.ImageStyles.FindAsync(id);
        return style is null ? NotFound() : Ok(style);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ImageStyle style)
    {
        db.ImageStyles.Add(style);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = style.StyleId }, style);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ImageStyle inputStyle)
    {
        var style = await db.ImageStyles.FindAsync(id);
        if (style is null) return NotFound();

        style.Description = inputStyle.Description;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var style = await db.ImageStyles.FindAsync(id);
        if (style is null) return NotFound();

        db.ImageStyles.Remove(style);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
