using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinterestApi.Data;
using PinterestApi.Models;

namespace PinterestApi.Controllers;

[ApiController]
[Route("api/roomtypes")]
public class RoomTypesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.ImageRoomTypes.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var roomType = await db.ImageRoomTypes.FindAsync(id);
        return roomType is null ? NotFound() : Ok(roomType);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ImageRoomType roomType)
    {
        db.ImageRoomTypes.Add(roomType);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = roomType.RoomTypeId }, roomType);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ImageRoomType inputRoomType)
    {
        var roomType = await db.ImageRoomTypes.FindAsync(id);
        if (roomType is null) return NotFound();

        roomType.Description = inputRoomType.Description;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var roomType = await db.ImageRoomTypes.FindAsync(id);
        if (roomType is null) return NotFound();

        db.ImageRoomTypes.Remove(roomType);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
