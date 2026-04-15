using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record DismissNotificationCommand(Guid NotificationId) : IRequest<Result<Notification>>;

public sealed class DismissNotificationCommandHandler : IRequestHandler<DismissNotificationCommand, Result<Notification>>
{
    private readonly INotificationRepository _repository;

    public DismissNotificationCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Notification>> Handle(DismissNotificationCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        NotificationId id = new(request.NotificationId);
        Notification? notification = await _repository.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (notification is null)
        {
            return new NotFoundError($"Notification with id '{request.NotificationId}' was not found.");
        }

        notification.Dismiss();
        await _repository.SaveAsync(notification, ct).ConfigureAwait(false);
        return notification;
    }
}
