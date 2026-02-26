using Microsoft.EntityFrameworkCore;
using ThrPresetsApi.Api.Common.Exceptions;
using ThrPresetsApi.Api.Data;
using ThrPresetsApi.Api.Features.Users.DTOs;

namespace ThrPresetsApi.Api.Features.Users;

public class UserService(AppDbContext db)
{
    public async Task<UserProfileDto> GetMeAsync(string userId)
    {
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.Presets)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User not found");

        return new UserProfileDto(
            user.Id,
            user.Email,
            user.Username,
            user.AvatarUrl,
            user.CreatedAt,
            user.Presets.Count
        );
    }

    public async Task<UserDto> UpdateMeAsync(string userId, UpdateUserDto dto)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found");

        if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.Username)
        {
            var exists = await db.Users.AnyAsync(u => u.Username == dto.Username);
            if (exists) throw new ConflictException("Username is already taken");
            user.Username = dto.Username;
        }

        if (dto.AvatarUrl != null)
        {
            user.AvatarUrl = dto.AvatarUrl;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new UserDto(user.Id, user.Username, user.AvatarUrl, user.CreatedAt);
    }

    public async Task DeleteMeAsync(string userId)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found");

        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }

    public async Task<PublicProfileDto> GetPublicProfileAsync(string username)
    {
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.Presets.Where(p => p.IsPublic))
            .FirstOrDefaultAsync(u => u.Username == username)
            ?? throw new NotFoundException("User not found");

        var userDto = new UserDto(user.Id, user.Username, user.AvatarUrl, user.CreatedAt);
        
        var presets = user.Presets.Select(p => new PresetSummaryDto(
            p.Id,
            p.Name,
            p.Slug,
            p.Downloads,
            p.WilsonScore
        ));

        return new PublicProfileDto(userDto, presets);
    }
}