using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using ThrPresetsApi.Api.Common.Exceptions;
using ThrPresetsApi.Api.Common.Utils;
using ThrPresetsApi.Api.Data;
using ThrPresetsApi.Api.Features.Presets.DTOs;
using ThrPresetsApi.Api.Infrastructure.S3;
using ThrPresetsApi.Api.Models;

namespace ThrPresetsApi.Api.Features.Presets;

public class PresetService(AppDbContext db, IS3Service s3)
{
    public async Task<IEnumerable<PresetSummaryDto>> GetPresetsAsync(PresetFilters filters)
    {
        var query = db.Presets.AsNoTracking()
            .Where(p => p.IsPublic);

        if (!string.IsNullOrWhiteSpace(filters.Search))
            query = query.Where(p => p.Name.Contains(filters.Search) || (p.Description != null && p.Description.Contains(filters.Search)));

        if (!string.IsNullOrWhiteSpace(filters.AuthorId))
            query = query.Where(p => p.AuthorId == filters.AuthorId);

        if (filters.TagIds is { Length: > 0 })
        {
            query = query.Where(p => p.Tags.Any(pt => filters.TagIds.Contains(pt.TagId)));
        }

        query = filters.SortBy switch
        {
            "downloads" => query.OrderByDescending(p => p.Downloads),
            "new" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.WilsonScore)
        };

        return await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(p => new PresetSummaryDto
            {
                Slug = p.Slug,
                Name = p.Name,
                Description = p.Description,
                Downloads = p.Downloads,
                WilsonScore = p.WilsonScore,
                AuthorName = p.Author != null ? p.Author.Username : "Guest",
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PresetDto> GetBySlugAsync(string slug)
    {
        var preset = await db.Presets
            .AsNoTracking()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Slug == slug)
            ?? throw new NotFoundException("Preset not found");

        return MapToDto(preset);
    }

    public async Task<PresetDto> CreateAsync(CreatePresetDto dto, string? userId)
    {
        var slug = await GenerateUniqueSlug(dto.Name);

        await using var stream = dto.File.OpenReadStream();
        var s3Key = await s3.UploadAsync(stream, dto.File.FileName, dto.File.ContentType);

        var preset = new Preset
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            Source = dto.Source,
            S3Key = s3Key,
            FileSize = (int)dto.File.Length,
            IsPublic = dto.IsPublic ?? true,
            AuthorId = userId,
            Tags = dto.TagIds?.Select(tagId => new PresetTag { TagId = tagId }).ToList() 
                   ?? []
        };

        db.Presets.Add(preset);
        await db.SaveChangesAsync();

        return MapToDto(preset);
    }

    public async Task<PresetDto> UpdateAsync(string slug, UpdatePresetDto dto, string userId)
    {
        var preset = await db.Presets
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Slug == slug)
            ?? throw new NotFoundException("Preset not found");

        if (preset.AuthorId != userId) throw new ForbiddenException("Cannot update others' presets");

        // Mandatory fields per UpdatePresetDtoValidator
        preset.Name = dto.Name!;
        preset.Description = dto.Description;
        preset.IsPublic = dto.IsPublic!.Value;
        preset.Source = dto.Source;
        preset.UpdatedAt = DateTime.UtcNow;

        if (dto.File is null) throw new BadRequestException("File is required.");
            await s3.DeleteAsync(preset.S3Key);
            await using var stream = dto.File.OpenReadStream();
            preset.S3Key = await s3.UploadAsync(stream, dto.File.FileName, dto.File.ContentType);
            preset.FileSize = (int)dto.File.Length;

        // Handle Tags Update
        if (dto.TagIds != null)
        {
            preset.Tags.Clear();
            foreach (var tagId in dto.TagIds)
            {
                preset.Tags.Add(new PresetTag { TagId = tagId });
            }
        }

        // Regen slug if name changed
        if (!string.Equals(preset.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
        {
            preset.Slug = await GenerateUniqueSlug(dto.Name!);
        }

        await db.SaveChangesAsync();
        return MapToDto(preset);
    }

    public async Task DeleteAsync(string slug, string userId)
    {
        var preset = await db.Presets.FirstOrDefaultAsync(p => p.Slug == slug)
            ?? throw new NotFoundException("Preset not found");

        if (preset.AuthorId != userId) throw new ForbiddenException("Cannot delete others' presets");

        await s3.DeleteAsync(preset.S3Key);
        db.Presets.Remove(preset);
        await db.SaveChangesAsync();
    }

    public async Task RateAsync(string slug, int stars, string userId)
    {
        if (stars is < 1 or > 5) throw new BadRequestException("Rating must be between 1 and 5 stars.");

        var preset = await db.Presets
            .Include(p => p.Ratings)
            .FirstOrDefaultAsync(p => p.Slug == slug)
            ?? throw new NotFoundException("Preset not found");

        var existingRating = preset.Ratings.FirstOrDefault(r => r.UserId == userId);

        if (existingRating != null)
        {
            existingRating.Stars = stars;
            existingRating.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            preset.Ratings.Add(new Rating { UserId = userId, Stars = stars, PresetId = preset.Id });
        }

        int total = preset.Ratings.Count;
        int positive = preset.Ratings.Count(r => r.Stars >= 4);
        preset.WilsonScore = RankingUtils.CalculateWilsonScore(positive, total);

        await db.SaveChangesAsync();
    }

    public async Task<string> GetDownloadLinkAsync(string slug)
    {
        var preset = await db.Presets.FirstOrDefaultAsync(p => p.Slug == slug)
            ?? throw new NotFoundException("Preset not found");

        preset.Downloads++;
        await db.SaveChangesAsync();

        return s3.GetDownloadUrl(preset.S3Key, $"{preset.Slug}.thrl6p");
    }

    private static PresetDto MapToDto(Preset p)
    {
        return new PresetDto
        {
            Id = p.Id,
            Slug = p.Slug,
            Name = p.Name,
            Description = p.Description,
            Source = p.Source,
            FileSize = p.FileSize,
            Downloads = p.Downloads,
            WilsonScore = p.WilsonScore,
            AuthorId = p.AuthorId,
            AuthorName = p.Author?.Username,
            CreatedAt = p.CreatedAt
        };
    }

    private async Task<string> GenerateUniqueSlug(string name)
    {
        var baseSlug = Regex.Replace(name.ToLower(), @"[^a-z0-9\s-]", "").Replace(" ", "-");
        var slug = baseSlug;
        var count = 1;

        while (await db.Presets.AnyAsync(p => p.Slug == slug))
        {
            slug = $"{baseSlug}-{count++}";
        }
        return slug;
    }
}