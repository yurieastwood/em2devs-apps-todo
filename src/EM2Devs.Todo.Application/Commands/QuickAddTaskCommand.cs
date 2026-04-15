using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;

namespace EM2Devs.Todo.Application.Commands;

/// <summary>
/// Creates a task from a single raw quick-add string containing inline directives
/// (<c>#tag</c>, <c>!priority</c>, <c>^date</c>) parsed by <see cref="QuickAddParser"/>.
/// </summary>
public sealed record QuickAddTaskCommand(string Input) : IRequest<Result<TodoTask>>;

public sealed class QuickAddTaskCommandHandler : IRequestHandler<QuickAddTaskCommand, Result<TodoTask>>
{
    private readonly ITaskRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public QuickAddTaskCommandHandler(ITaskRepository repository, ICurrentUser currentUser, TimeProvider timeProvider)
    {
        _repository = repository;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Result<TodoTask>> Handle(QuickAddTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        QuickAddResult parsed;
        try
        {
            parsed = QuickAddParser.Parse(request.Input, today);
        }
        catch (DomainException ex)
        {
            return new ValidationError(ex.Message);
        }

        TodoTask task = TodoTask.Create(
            _currentUser.UserId,
            parsed.Title,
            priority: parsed.Priority ?? Domain.ValueObjects.TaskPriority.Medium,
            scheduledDate: parsed.DueDate);

        foreach (var tag in parsed.Tags)
        {
            task.AddTag(tag);
        }

        await _repository.SaveAsync(task, ct).ConfigureAwait(false);
        return task;
    }
}
