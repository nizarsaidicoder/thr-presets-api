using System.ComponentModel.DataAnnotations;

namespace ThrPresetsApi.Api.Features.Auth.DTOs;

public class SignInDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
