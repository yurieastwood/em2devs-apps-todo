using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Domain.ValueObjects;
using FluentValidation;

namespace EM2Devs.Todo.Application.Validators;

/// <summary>
/// Bounds the freeze duration to the domain's supported range.
/// <see cref="StreakFreeze.MaxFreezeDuration"/> caps it at 7 days.
/// </summary>
public sealed class FreezeStreakCommandValidator : AbstractValidator<FreezeStreakCommand>
{
    public FreezeStreakCommandValidator()
    {
        RuleFor(x => x.Days)
            .InclusiveBetween(1, StreakFreeze.MaxFreezeDuration)
            .WithMessage($"Days must be between 1 and {StreakFreeze.MaxFreezeDuration}.");
    }
}
