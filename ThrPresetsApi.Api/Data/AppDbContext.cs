namespace ThrPresetsApi.Api.Data;

using Microsoft.EntityFrameworkCore;
using ThrPresetsApi.Api.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Preset> Presets => Set<Preset>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PresetTag> PresetTags => Set<PresetTag>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionItem> CollectionItems => Set<CollectionItem>();
    public DbSet<PresetReport> PresetReports => Set<PresetReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

        // ── Preset ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Preset>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.Slug).IsUnique();
            e.HasIndex(p => p.Downloads);
            e.HasIndex(p => p.WilsonScore);
            e.HasIndex(p => p.CreatedAt);
            e.HasIndex(p => p.IsPublic);
            e.HasIndex(p => p.AuthorId);

            e.HasOne(p => p.Author)
             .WithMany(u => u.Presets)
             .HasForeignKey(p => p.AuthorId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── PresetTag  ──────────────────────────────────────────
        modelBuilder.Entity<PresetTag>(e =>
        {
            e.HasKey(pt => new { pt.PresetId, pt.TagId });

            e.HasOne(pt => pt.Preset)
             .WithMany(p => p.Tags)
             .HasForeignKey(pt => pt.PresetId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pt => pt.Tag)
             .WithMany(t => t.Presets)
             .HasForeignKey(pt => pt.TagId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Rating ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Rating>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => new { r.UserId, r.PresetId }).IsUnique();
            e.HasIndex(r => r.PresetId);

            e.HasOne(r => r.User)
             .WithMany(u => u.Ratings)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Preset)
             .WithMany(p => p.Ratings)
             .HasForeignKey(r => r.PresetId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(r => r.Stars)
             .IsRequired();
        });

        // ── Favorite  ───────────────────────────────────────────
        modelBuilder.Entity<Favorite>(e =>
        {
            e.HasKey(f => new { f.UserId, f.PresetId });
            e.HasIndex(f => f.UserId);

            e.HasOne(f => f.User)
             .WithMany(u => u.Favorites)
             .HasForeignKey(f => f.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(f => f.Preset)
             .WithMany(p => p.Favorites)
             .HasForeignKey(f => f.PresetId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Collection ────────────────────────────────────────────────────────
        modelBuilder.Entity<Collection>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.UserId);

            e.HasOne(c => c.User)
             .WithMany(u => u.Collections)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CollectionItem ─────────────────────────────────────
        modelBuilder.Entity<CollectionItem>(e =>
        {
            e.HasKey(ci => new { ci.CollectionId, ci.PresetId });

            e.HasOne(ci => ci.Collection)
             .WithMany(c => c.Items)
             .HasForeignKey(ci => ci.CollectionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ci => ci.Preset)
             .WithMany(p => p.CollectionItems)
             .HasForeignKey(ci => ci.PresetId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── PresetReport ──────────────────────────────────────────────────────
        modelBuilder.Entity<PresetReport>(e =>
        {
            e.HasKey(pr => pr.Id);
            e.HasIndex(pr => pr.Resolved);
            e.HasIndex(pr => pr.PresetId);

            e.HasOne(pr => pr.Reporter)
             .WithMany(u => u.Reports)
             .HasForeignKey(pr => pr.ReporterId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(pr => pr.Preset)
             .WithMany(p => p.Reports)
             .HasForeignKey(pr => pr.PresetId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Enums stored as strings ───────────────────────────────────────────
        modelBuilder.Entity<Tag>()
            .Property(t => t.Type)
            .HasConversion<string>();

        modelBuilder.Entity<PresetReport>()
            .Property(pr => pr.Reason)
            .HasConversion<string>();
    }
}