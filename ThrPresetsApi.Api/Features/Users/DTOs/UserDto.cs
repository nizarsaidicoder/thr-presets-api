namespace ThrPresetsApi.Api.Features.Users.DTOs;

public record UserDto(
    string Id,
    string Username,
    string? AvatarUrl,
    DateTime CreatedAt
);