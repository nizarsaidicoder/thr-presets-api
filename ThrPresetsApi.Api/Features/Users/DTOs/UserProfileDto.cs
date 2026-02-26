namespace ThrPresetsApi.Api.Features.Users.DTOs;

public record UserProfileDto(
    string Id,
    string Email,
    string Username,
    string? AvatarUrl,
    DateTime CreatedAt,
    int PresetsCount
);