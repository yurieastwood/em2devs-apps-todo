using EM2Devs.Todo.Application.Commands;
using FluentValidation;

namespace EM2Devs.Todo.Application.Validators;

public sealed class RecordActualTimeCommandValidator : AbstractValidator<RecordActualTimeCommand>
{
    public RecordActualTimeCommandValidator()
    {
        RuleFor(x => x.ActualMinutes)
            .InclusiveBetween(1, 1440)
            .WithMessage("Actual minutes must be between 1 and 1440 (24 hours).");
    }
}
