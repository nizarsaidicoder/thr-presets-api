using Microsoft.AspNetCore.Mvc;

namespace ThrPresetsApi.Api.Features.Presets.DTOs;

public class PresetFilters
{
    public string? Search { get; init; }
    public string? AuthorId { get; init; }

    [FromQuery(Name = "tagIds")]
    public string[]? TagIds { get; init; }

    public string? SortBy { get; init; } = "wilson";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}