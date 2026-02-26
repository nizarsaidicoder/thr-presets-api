using ThrPresetsApi.Api.Features.Users.DTOs;

namespace ThrPresetsApi.Api.Features.Auth.DTOs;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}
