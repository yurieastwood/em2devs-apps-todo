using EM2Devs.Todo.Application.Queries;
using FluentValidation;

namespace EM2Devs.Todo.Application.Validators;

public sealed class ListTasksQueryValidator : AbstractValidator<ListTasksQuery>
{
    private static readonly HashSet<string> _validStatusNames =
        new(Enum.GetNames<Domain.TaskStatus>(), StringComparer.Ordinal);

    public ListTasksQueryValidator()
    {
        RuleFor(x => x.StatusFilter)
            .Must(status => status is null || _validStatusNames.Contains(status))
            .WithMessage(x => $"Invalid status filter '{x.StatusFilter}'. Valid values: Todo, InProgress, Done.");
    }
}
