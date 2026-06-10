using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinterestApi.Data;
using PinterestApi.Models;

namespace PinterestApi.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.ImageCategories.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await db.ImageCategories.FindAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ImageCategory category)
    {
        db.ImageCategories.Add(category);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ImageCategory inputCategory)
    {
        var category = await db.ImageCategories.FindAsync(id);
        if (category is null) return NotFound();

        category.Description = inputCategory.Description;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.ImageCategories.FindAsync(id);
        if (category is null) return NotFound();

        db.ImageCategories.Remove(category);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
