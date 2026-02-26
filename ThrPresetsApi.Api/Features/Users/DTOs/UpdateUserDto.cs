namespace ThrPresetsApi.Api.Features.Users.DTOs;

public record UpdateUserDto(
    string? Username,
    string? AvatarUrl
);