namespace ThrPresetsApi.Api.Common.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    /// Require authentication for this endpoint or group.
    /// </summary>
    public static TBuilder JwtAuthGuard<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization();
    }

    /// <summary>
    /// Allow both guests and authenticated users.
    /// </summary>
    public static TBuilder OptionalJwtGuard<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        return builder.AllowAnonymous();
    }

    /// <summary>
    /// Mark this endpoint or group as private (requires authentication).
    /// </summary>
    public static TBuilder RequireAuth<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization();
    }

    /// <summary>
    /// Mark this endpoint or group as public (no authentication required).
    /// </summary>
    public static TBuilder IsPublic<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        return builder.AllowAnonymous();
    }
}
