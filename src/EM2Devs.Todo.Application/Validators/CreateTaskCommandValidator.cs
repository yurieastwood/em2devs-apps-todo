using EM2Devs.Todo.Application.Commands;
using FluentValidation;

namespace EM2Devs.Todo.Application.Validators;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
    }
}
