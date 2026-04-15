using EM2Devs.Todo.Application.Commands;
using FluentValidation;

namespace EM2Devs.Todo.Application.Validators;

public sealed class SaveWeeklyReviewCommandValidator : AbstractValidator<SaveWeeklyReviewCommand>
{
    public const int MaxReflectionLength = 4000;

    public SaveWeeklyReviewCommandValidator()
    {
        RuleFor(x => x.WhatWentWell)
            .NotEmpty().WithMessage("What went well is required.")
            .MaximumLength(MaxReflectionLength)
            .WithMessage($"What went well must not exceed {MaxReflectionLength} characters.");

        RuleFor(x => x.WhatDragged)
            .NotEmpty().WithMessage("What dragged is required.")
            .MaximumLength(MaxReflectionLength)
            .WithMessage($"What dragged must not exceed {MaxReflectionLength} characters.");

        RuleFor(x => x.Adjustment)
            .NotEmpty().WithMessage("Adjustment is required.")
            .MaximumLength(MaxReflectionLength)
            .WithMessage($"Adjustment must not exceed {MaxReflectionLength} characters.");
    }
}
