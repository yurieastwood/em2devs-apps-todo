using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Domain.Entities;
using FluentValidation;

namespace EM2Devs.Todo.Application.Validators;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>Minimum password length for dev-grade policy (Phase 0).</summary>
    public const int MinPasswordLength = 8;

    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(User.MaxEmailLength)
                .WithMessage($"Email must not exceed {User.MaxEmailLength} characters.")
            .EmailAddress().WithMessage("Email must be a valid address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(MinPasswordLength)
                .WithMessage($"Password must be at least {MinPasswordLength} characters.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MinimumLength(User.MinDisplayNameLength)
                .WithMessage($"Display name must be at least {User.MinDisplayNameLength} character.")
            .MaximumLength(User.MaxDisplayNameLength)
                .WithMessage($"Display name must not exceed {User.MaxDisplayNameLength} characters.");
    }
}
