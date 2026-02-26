namespace ThrPresetsApi.Api.Features.Users.DTOs;

public record PresetSummaryDto(
    string Id,
    string Name,
    string Slug,
    int Downloads,
    double WilsonScore
);