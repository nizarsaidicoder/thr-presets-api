namespace ThrPresetsApi.Api.Features.Presets.DTOs;

public class PresetDto
{
    public string Id { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public string? Source { get; init; }
    public int FileSize { get; init; }
    public int Downloads { get; init; }
    public double WilsonScore { get; init; }
    public string? AuthorId { get; init; }
    public string? AuthorName { get; init; }
    public DateTime CreatedAt { get; init; }
}