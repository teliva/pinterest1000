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

        // Configure column names for snake_case SQL schema
        modelBuilder.Entity<ImageCategory>(entity =>
        {
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<ImageRoomType>(entity =>
        {
            entity.Property(e => e.RoomTypeId).HasColumnName("room_type_id");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<ImageStyle>(entity =>
        {
            entity.Property(e => e.StyleId).HasColumnName("style_id");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.RoomTypeId).HasColumnName("room_type_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

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
            .HasMany(i => i.Styles)
            .WithMany(s => s.Images)
            .UsingEntity<Dictionary<string, object>>(
                "image_image_style",
                j => j
                    .HasOne<ImageStyle>()
                    .WithMany()
                    .HasForeignKey("style_id")
                    .OnDelete(DeleteBehavior.NoAction),
                j => j
                    .HasOne<Image>()
                    .WithMany()
                    .HasForeignKey("image_id")
                    .OnDelete(DeleteBehavior.NoAction),
                j =>
                {
                    j.ToTable("image_image_style");
                    j.HasKey("image_id", "style_id");
                    j.Property<Guid>("image_id").HasColumnName("image_id");
                    j.Property<int>("style_id").HasColumnName("style_id");
                });

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
    }
}