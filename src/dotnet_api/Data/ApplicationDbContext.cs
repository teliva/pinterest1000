using Microsoft.EntityFrameworkCore;
using PinterestApi.Models;

namespace PinterestApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ImageCategory> ImageCategories { get; set; }
    public DbSet<ImageRoomType> ImageRoomTypes { get; set; }
    public DbSet<ImageStyle> ImageStyles { get; set; }
    public DbSet<Image> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table names to match the database
        modelBuilder.Entity<ImageCategory>().ToTable("image_categories");
        modelBuilder.Entity<ImageRoomType>().ToTable("image_room_type");
        modelBuilder.Entity<ImageStyle>().ToTable("image_style");
        modelBuilder.Entity<Image>().ToTable("image");

        // Configure primary keys
        modelBuilder.Entity<ImageCategory>().HasKey(c => c.CategoryId);
        modelBuilder.Entity<ImageRoomType>().HasKey(r => r.RoomTypeId);
        modelBuilder.Entity<ImageStyle>().HasKey(s => s.StyleId);
        modelBuilder.Entity<Image>().HasKey(i => i.Id);

        // Configure relationships
        modelBuilder.Entity<Image>()
            .HasOne(i => i.Category)
            .WithMany(c => c.Images)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Image>()
            .HasOne(i => i.RoomType)
            .WithMany(r => r.Images)
            .HasForeignKey(i => i.RoomTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Image>()
            .HasOne(i => i.Style)
            .WithMany(s => s.Images)
            .HasForeignKey(i => i.StyleId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure unique constraints
        modelBuilder.Entity<ImageCategory>()
            .HasIndex(c => c.Description)
            .IsUnique();

        modelBuilder.Entity<ImageRoomType>()
            .HasIndex(r => r.Description)
            .IsUnique();

        modelBuilder.Entity<ImageStyle>()
            .HasIndex(s => s.Description)
            .IsUnique();

        // Configure indexes
        modelBuilder.Entity<Image>()
            .HasIndex(i => i.CategoryId)
            .HasDatabaseName("idx_image_category_id");

        modelBuilder.Entity<Image>()
            .HasIndex(i => i.RoomTypeId)
            .HasDatabaseName("idx_image_room_type_id");

        modelBuilder.Entity<Image>()
            .HasIndex(i => i.StyleId)
            .HasDatabaseName("idx_image_style_id");
    }
}