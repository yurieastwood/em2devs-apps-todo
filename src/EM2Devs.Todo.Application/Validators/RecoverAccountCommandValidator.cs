using EM2Devs.Todo.Application.Commands;
using FluentValidation;

namespace EM2Devs.Todo.Application.Validators;

public sealed class RecoverAccountCommandValidator : AbstractValidator<RecoverAccountCommand>
{
    public RecoverAccountCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}
