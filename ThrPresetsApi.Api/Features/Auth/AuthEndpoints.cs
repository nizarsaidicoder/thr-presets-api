using Microsoft.AspNetCore.Mvc;
using ThrPresetsApi.Api.Common.Extensions;
using ThrPresetsApi.Api.Common.Filters;
using ThrPresetsApi.Api.Features.Auth.DTOs;

namespace ThrPresetsApi.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/signup", SignUp)
            .WithName("SignUp")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new account and returns an access token. A refresh token is set in an httpOnly cookie.")
            .Produces<AuthResponseDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .AddEndpointFilter<ValidationFilter<SignUpDto>>()
            .IsPublic();

        group.MapPost("/signin", SignIn)
            .WithName("SignIn")
            .WithSummary("Sign in")
            .WithDescription("Authenticates a user and returns an access token. A refresh token is set in an httpOnly cookie.")
            .Produces<AuthResponseDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .AddEndpointFilter<ValidationFilter<SignInDto>>()
            .IsPublic();

        group.MapPost("/refresh", Refresh)
            .WithName("Refresh")
            .WithSummary("Refresh access token")
            .WithDescription("Uses the httpOnly refresh token cookie to issue a new access token.")
            .Produces<RefreshResponseDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .IsPublic();

        group.MapPost("/logout", SignOut)
            .WithName("Logout")
            .WithSummary("Logout")
            .WithDescription("Clears the refresh token cookie.")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuth();
    }

    private static async Task<IResult> SignUp(
        [FromBody] SignUpDto dto,
        AuthService authService,
        TokenService tokenService,
        HttpResponse response)
    {
        var (result, user) = await authService.SignUpAsync(dto);
        tokenService.SetRefreshTokenCookie(response, tokenService.GenerateRefreshToken(user));
        return TypedResults.Created("/api/users/me", result);
    }

    private static async Task<IResult> SignIn(
        [FromBody] SignInDto dto,
        AuthService authService,
        TokenService tokenService,
        HttpResponse response)
    {
        var (result, user) = await authService.SignInAsync(dto);
        tokenService.SetRefreshTokenCookie(response, tokenService.GenerateRefreshToken(user));
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> Refresh(
        HttpRequest request,
        AuthService authService)
    {
        var refreshToken = request.Cookies["refresh_token"];
        var accessToken = await authService.RefreshAsync(refreshToken);
        return TypedResults.Ok(new RefreshResponseDto { AccessToken = accessToken });
    }

    private static IResult SignOut(
        TokenService tokenService,
        HttpResponse response)
    {
        tokenService.ClearRefreshTokenCookie(response);
        return TypedResults.NoContent();
    }
}