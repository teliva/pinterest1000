using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PinterestApi.Data;
using PinterestApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8082")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.MapGet("/", () => Results.Ok(new { service = "pinterest_dotnet_api", status = "running" }));

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.MapGet("/dbtest", async (ApplicationDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { connected = true, database = "ImageDatabase" });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

// Image Categories endpoints
app.MapGet("/api/categories", async (ApplicationDbContext db) =>
    await db.ImageCategories.ToListAsync());

app.MapGet("/api/categories/{id}", async (int id, ApplicationDbContext db) =>
    await db.ImageCategories.FindAsync(id) is ImageCategory category
        ? Results.Ok(category)
        : Results.NotFound());

app.MapPost("/api/categories", async (ImageCategory category, ApplicationDbContext db) =>
{
    db.ImageCategories.Add(category);
    await db.SaveChangesAsync();
    return Results.Created($"/api/categories/{category.CategoryId}", category);
});

app.MapPut("/api/categories/{id}", async (int id, ImageCategory inputCategory, ApplicationDbContext db) =>
{
    var category = await db.ImageCategories.FindAsync(id);
    if (category is null) return Results.NotFound();

    category.Description = inputCategory.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/categories/{id}", async (int id, ApplicationDbContext db) =>
{
    var category = await db.ImageCategories.FindAsync(id);
    if (category is null) return Results.NotFound();

    db.ImageCategories.Remove(category);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Image Room Types endpoints
app.MapGet("/api/roomtypes", async (ApplicationDbContext db) =>
    await db.ImageRoomTypes.ToListAsync());

app.MapGet("/api/roomtypes/{id}", async (int id, ApplicationDbContext db) =>
    await db.ImageRoomTypes.FindAsync(id) is ImageRoomType roomType
        ? Results.Ok(roomType)
        : Results.NotFound());

app.MapPost("/api/roomtypes", async (ImageRoomType roomType, ApplicationDbContext db) =>
{
    db.ImageRoomTypes.Add(roomType);
    await db.SaveChangesAsync();
    return Results.Created($"/api/roomtypes/{roomType.RoomTypeId}", roomType);
});

app.MapPut("/api/roomtypes/{id}", async (int id, ImageRoomType inputRoomType, ApplicationDbContext db) =>
{
    var roomType = await db.ImageRoomTypes.FindAsync(id);
    if (roomType is null) return Results.NotFound();

    roomType.Description = inputRoomType.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/roomtypes/{id}", async (int id, ApplicationDbContext db) =>
{
    var roomType = await db.ImageRoomTypes.FindAsync(id);
    if (roomType is null) return Results.NotFound();

    db.ImageRoomTypes.Remove(roomType);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Image Styles endpoints
app.MapGet("/api/styles", async (ApplicationDbContext db) =>
    await db.ImageStyles.ToListAsync());

app.MapGet("/api/styles/{id}", async (int id, ApplicationDbContext db) =>
    await db.ImageStyles.FindAsync(id) is ImageStyle style
        ? Results.Ok(style)
        : Results.NotFound());

app.MapPost("/api/styles", async (ImageStyle style, ApplicationDbContext db) =>
{
    db.ImageStyles.Add(style);
    await db.SaveChangesAsync();
    return Results.Created($"/api/styles/{style.StyleId}", style);
});

app.MapPut("/api/styles/{id}", async (int id, ImageStyle inputStyle, ApplicationDbContext db) =>
{
    var style = await db.ImageStyles.FindAsync(id);
    if (style is null) return Results.NotFound();

    style.Description = inputStyle.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/styles/{id}", async (int id, ApplicationDbContext db) =>
{
    var style = await db.ImageStyles.FindAsync(id);
    if (style is null) return Results.NotFound();

    db.ImageStyles.Remove(style);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Images endpoints
app.MapGet("/api/images", async (ApplicationDbContext db) =>
    await db.Images.Include(i => i.Category)
                   .Include(i => i.RoomType)
                   .Include(i => i.Styles)
                   .ToListAsync());

app.MapGet("/api/images/{id}", async (Guid id, ApplicationDbContext db) =>
    await db.Images.Include(i => i.Category)
                   .Include(i => i.RoomType)
                   .Include(i => i.Styles)
                   .FirstOrDefaultAsync(i => i.Id == id) is Image image
        ? Results.Ok(image)
        : Results.NotFound());

app.MapPost("/api/images", async (Image inputImage, ApplicationDbContext db) =>
{
    if (inputImage.StyleIds is not null && inputImage.StyleIds.Any())
    {
        var styles = await db.ImageStyles
            .Where(s => inputImage.StyleIds.Contains(s.StyleId))
            .ToListAsync();
        inputImage.Styles = styles;
    }

    db.Images.Add(inputImage);
    await db.SaveChangesAsync();
    return Results.Created($"/api/images/{inputImage.Id}", inputImage);
});

app.MapPut("/api/images/{id}", async (Guid id, Image inputImage, ApplicationDbContext db) =>
{
    var image = await db.Images.Include(i => i.Styles).FirstOrDefaultAsync(i => i.Id == id);
    if (image is null) return Results.NotFound();

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
        {
            image.Styles.Add(style);
        }
    }

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/images/{id}", async (Guid id, ApplicationDbContext db) =>
{
    var image = await db.Images.FindAsync(id);
    if (image is null) return Results.NotFound();

    db.Images.Remove(image);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
