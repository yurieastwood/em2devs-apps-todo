using EM2Devs.Todo.Application.Commands;
using FluentValidation;

namespace EM2Devs.Todo.Application.Validators;

public sealed class QuickAddTaskCommandValidator : AbstractValidator<QuickAddTaskCommand>
{
    public QuickAddTaskCommandValidator()
    {
        RuleFor(x => x.Input)
            .NotEmpty().WithMessage("Input is required.")
            .MaximumLength(500).WithMessage("Input must not exceed 500 characters.");
    }
}
