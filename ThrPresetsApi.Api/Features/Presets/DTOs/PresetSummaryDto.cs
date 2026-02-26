namespace ThrPresetsApi.Api.Features.Presets.DTOs;

public class PresetSummaryDto
{
    public string Slug { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public int Downloads { get; init; }
    public double WilsonScore { get; init; }
    public string? AuthorName { get; init; }
    public DateTime CreatedAt { get; init; }
}