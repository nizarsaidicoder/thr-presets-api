namespace ThrPresetsApi.Api.Features.Users.DTOs;

public record PublicProfileDto(
    UserDto User,
    IEnumerable<PresetSummaryDto> Presets
);