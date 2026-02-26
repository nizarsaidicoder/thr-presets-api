using FluentValidation;
using ThrPresetsApi.Api.Features.Auth.DTOs;

namespace ThrPresetsApi.Api.Features.Auth.Validators;

public class SignInDtoValidator : AbstractValidator<SignInDto>
{
    public SignInDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}