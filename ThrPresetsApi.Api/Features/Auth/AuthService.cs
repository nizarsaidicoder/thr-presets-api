using Microsoft.EntityFrameworkCore;
using ThrPresetsApi.Api.Common.Exceptions;
using ThrPresetsApi.Api.Data;
using ThrPresetsApi.Api.Features.Auth.DTOs;
using ThrPresetsApi.Api.Features.Users.DTOs;
using ThrPresetsApi.Api.Models;

namespace ThrPresetsApi.Api.Features.Auth;

public class AuthService(AppDbContext db, TokenService tokenService)
{
    public async Task<(AuthResponseDto Response, User User)> SignUpAsync(SignUpDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email.ToLowerInvariant()))
            throw new ConflictException("Email already in use");

        if (await db.Users.AnyAsync(u => u.Username == dto.Username))
            throw new ConflictException("Username already taken");

        var user = new User
        {
            Email = dto.Email.ToLowerInvariant(),
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (BuildAuthResponse(user), user);
    }

    public async Task<(AuthResponseDto Response, User User)> SignInAsync(SignInDto dto)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLowerInvariant());

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new ValidationException("Invalid email or password");

        return (BuildAuthResponse(user), user);
    }

    public async Task<string> RefreshAsync(string? refreshToken)
    {
        if (refreshToken is null)
            throw new ValidationException("Refresh token missing");

        var principal = tokenService.ValidateRefreshToken(refreshToken);
        if (principal is null)
            throw new ValidationException("Invalid or expired refresh token");

        var userId = principal.FindFirst("sub")?.Value;
        var user = await db.Users.FindAsync(userId);

        if (user is null)
            throw new NotFoundException("User not found");

        return tokenService.GenerateAccessToken(user);
    }

    private AuthResponseDto BuildAuthResponse(User user) => new()
    {
        AccessToken = tokenService.GenerateAccessToken(user),
        User = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            AvatarUrl = user.AvatarUrl,
        }
    };
}
