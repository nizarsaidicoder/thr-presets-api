using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ThrPresetsApi.Api.Common.Extensions;
using ThrPresetsApi.Api.Common.Filters;
using ThrPresetsApi.Api.Features.Users.DTOs;

namespace ThrPresetsApi.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("/me", GetMe)
            .WithName("GetMyProfile")
            .WithSummary("Get current user profile")
            .WithDescription("Returns the private profile details of the authenticated user.")
            .Produces<UserProfileDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .RequireAuth();

        group.MapPatch("/me", UpdateMe)
            .WithName("UpdateMyProfile")
            .WithSummary("Update profile")
            .WithDescription("Updates the username or avatar for the current user.")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .AddEndpointFilter<ValidationFilter<UpdateUserDto>>()
            .RequireAuth();

        group.MapDelete("/me", DeleteMe)
            .WithName("DeleteMyAccount")
            .WithSummary("Delete account")
            .WithDescription("Permanently removes the user account and associated data.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .RequireAuth();

        group.MapGet("/{username}", GetPublicProfile)
            .WithName("GetPublicProfile")
            .WithSummary("Get public profile")
            .WithDescription("Returns public user information and their presets by username.")
            .Produces<PublicProfileDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .IsPublic();
    }

    private static async Task<IResult> GetMe(
        ClaimsPrincipal principal,
        UserService userService)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await userService.GetMeAsync(userId);
        return TypedResults.Ok(profile);
    }

    private static async Task<IResult> UpdateMe(
        ClaimsPrincipal principal,
        UpdateUserDto dto,
        UserService userService)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var updatedUser = await userService.UpdateMeAsync(userId, dto);
        return TypedResults.Ok(updatedUser);
    }

    private static async Task<IResult> DeleteMe(
        ClaimsPrincipal principal,
        UserService userService)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await userService.DeleteMeAsync(userId);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetPublicProfile(
        string username,
        UserService userService)
    {
        var profile = await userService.GetPublicProfileAsync(username);
        return TypedResults.Ok(profile);
    }
}