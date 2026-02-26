using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ThrPresetsApi.Api.Models;

namespace ThrPresetsApi.Api.Features.Auth;

public class TokenService(IConfiguration config)
{
    private readonly string _secret = config["Jwt:Secret"]!;
    private readonly string _issuer = config["Jwt:Issuer"]!;
    private readonly string _audience = config["Jwt:Audience"]!;
    private readonly int _accessTokenExpiresInMinutes = int.Parse(config["Jwt:AccessTokenExpiresInMinutes"]!);
    private readonly int _refreshTokenExpiresInDays = int.Parse(config["Jwt:RefreshTokenExpiresInDays"]!);

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return BuildToken(claims, TimeSpan.FromMinutes(_accessTokenExpiresInMinutes));
    }

    public string GenerateRefreshToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return BuildToken(claims, TimeSpan.FromDays(_refreshTokenExpiresInDays));
    }

    public ClaimsPrincipal? ValidateRefreshToken(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetKey(),
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, parameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public void SetRefreshTokenCookie(HttpResponse response, string refreshToken)
    {
        response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(_refreshTokenExpiresInDays),
            Path = "/",
        });
    }

    public void ClearRefreshTokenCookie(HttpResponse response)
    {
        response.Cookies.Delete("refresh_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
        });
    }

    private string BuildToken(Claim[] claims, TimeSpan expiry)
    {
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: new SigningCredentials(GetKey(), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private SymmetricSecurityKey GetKey() =>
        new(Encoding.UTF8.GetBytes(_secret));
}
