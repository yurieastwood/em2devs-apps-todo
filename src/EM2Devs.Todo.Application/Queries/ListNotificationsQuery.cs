using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Queries;

/// <summary>
/// Lists notifications belonging to the current user. Dismissed notifications are
/// always hidden; read notifications are included only when <paramref name="IncludeRead"/>
/// is true. Results are ordered newest-first.
/// </summary>
public sealed record ListNotificationsQuery(bool IncludeRead = false) : IRequest<Result<IReadOnlyList<Notification>>>;

public sealed class ListNotificationsQueryHandler : IRequestHandler<ListNotificationsQuery, Result<IReadOnlyList<Notification>>>
{
    private readonly INotificationRepository _repository;

    public ListNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<Notification>>> Handle(ListNotificationsQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<Notification> items = await _repository
            .GetForCurrentUserAsync(request.IncludeRead, ct)
            .ConfigureAwait(false);

        IReadOnlyList<Notification> ordered = items
            .OrderByDescending(n => n.CreatedAt)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<Notification>>.Success(ordered);
    }
}
