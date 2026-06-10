using Microsoft.AspNetCore.Mvc;
using PinterestApi.Data;

namespace PinterestApi.Controllers;

[ApiController]
public class HealthController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("/")]
    public IActionResult Root() => Ok(new { service = "pinterest_dotnet_api", status = "running" });

    [HttpGet("/health")]
    public IActionResult Health() => Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });

    [HttpGet("/dbtest")]
    public async Task<IActionResult> DbTest()
    {
        try
        {
            await db.Database.CanConnectAsync();
            return Ok(new { connected = true, database = "ImageDatabase" });
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }
}
d