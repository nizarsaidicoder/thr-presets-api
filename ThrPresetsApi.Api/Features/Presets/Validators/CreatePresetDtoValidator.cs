using System;
using FluentValidation;
using ThrPresetsApi.Api.Features.Presets.DTOs;

namespace ThrPresetsApi.Api.Features.Presets.Validators;

public class CreatePresetDtoValidator : AbstractValidator<CreatePresetDto>
{
    public CreatePresetDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Source)
            .MaximumLength(200).WithMessage("Source must not exceed 200 characters");

        RuleFor(x => x.File)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("File is required")
            .Must(file => file.Length > 0).WithMessage("File cannot be empty")
            .Must(file => file.Length < 1024 * 1024).WithMessage("File size must be less than 1MB");
    }
}