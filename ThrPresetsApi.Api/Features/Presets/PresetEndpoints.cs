using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ThrPresetsApi.Api.Common.Extensions;
using ThrPresetsApi.Api.Common.Filters;
using ThrPresetsApi.Api.Features.Presets.DTOs;

namespace ThrPresetsApi.Api.Features.Presets;

public static class PresetEndpoints
{
    public static void MapPresetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/presets")
            .WithTags("Presets");

        group.MapGet("/", GetPresets)
            .WithName("GetPresets")
            .WithSummary("Browse presets")
            .Produces<IEnumerable<PresetSummaryDto>>(StatusCodes.Status200OK)
            .IsPublic();

        group.MapGet("/{slug}", GetPreset)
            .WithName("GetPreset")
            .WithSummary("Get preset details")
            .Produces<PresetDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .IsPublic();

        group.MapPost("/", CreatePreset)
            .WithName("CreatePreset")
            .WithSummary("Upload a preset")
            .Produces<PresetDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .AddEndpointFilter<ValidationFilter<CreatePresetDto>>()
            .DisableAntiforgery()
            .IsPublic();

        group.MapPut("/{slug}", UpdatePreset)
            .WithName("UpdatePreset")
            .WithSummary("Update a preset")
            .WithDescription("Updates preset metadata or file. Requires ownership.")
            .Produces<PresetDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .AddEndpointFilter<ValidationFilter<UpdatePresetDto>>()
            .DisableAntiforgery()
            .RequireAuth();

        group.MapDelete("/{slug}", DeletePreset)
            .WithName("DeletePreset")
            .WithSummary("Delete a preset")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .RequireAuth();
        
        group.MapPost("/{slug}/rate", RatePreset)
            .WithName("RatePreset")
            .WithSummary("Rate a preset")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .RequireAuth();

        group.MapGet("/{slug}/download", DownloadPreset)
            .WithName("DownloadPreset")
            .WithSummary("Download preset file")
            .Produces(StatusCodes.Status302Found)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .IsPublic();
    }

    private static async Task<IResult> GetPresets(
        [AsParameters] PresetFilters filters, 
        PresetService service)
    {
        var presets = await service.GetPresetsAsync(filters);
        return TypedResults.Ok(presets);
    }

    private static async Task<IResult> GetPreset(
        string slug, 
        PresetService service)
    {
        var preset = await service.GetBySlugAsync(slug);
        return TypedResults.Ok(preset);
    }

    private static async Task<IResult> CreatePreset(
        [FromForm] CreatePresetDto dto,
        ClaimsPrincipal principal,
        PresetService service)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var preset = await service.CreateAsync(dto, userId);
        return TypedResults.Created($"/api/presets/{preset.Slug}", preset);
    }

    private static async Task<IResult> UpdatePreset(
        string slug,
        [FromForm] UpdatePresetDto dto,
        ClaimsPrincipal principal,
        PresetService service)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var preset = await service.UpdateAsync(slug, dto, userId);
        return TypedResults.Ok(preset);
    }

    private static async Task<IResult> DeletePreset(
        string slug,
        ClaimsPrincipal principal,
        PresetService service)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await service.DeleteAsync(slug, userId);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> RatePreset(
        string slug,
        [FromBody] RatePresetDto dto,
        ClaimsPrincipal principal,
        PresetService service)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await service.RateAsync(slug, dto.Value, userId);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> DownloadPreset(
        string slug,
        PresetService service)
    {
        var url = await service.GetDownloadLinkAsync(slug);
        return TypedResults.Redirect(url);
    }
}