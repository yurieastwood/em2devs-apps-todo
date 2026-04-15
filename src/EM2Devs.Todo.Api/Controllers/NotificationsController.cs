using Asp.Versioning;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/notifications")]
[Route("api/v{version:apiVersion}/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lists notifications for the authenticated user. Dismissed notifications are
    /// always excluded. By default read notifications are also excluded; pass
    /// <c>?includeRead=true</c> to include them.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListNotifications(
        [FromQuery] bool includeRead = false,
        CancellationToken ct = default)
    {
        Result<IReadOnlyList<Notification>> result = await _mediator
            .Send(new ListNotificationsQuery(includeRead), ct).ConfigureAwait(false);
        return result.ToHttpResult(items => Ok(items.Select(Map)));
    }

    [HttpPost("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken ct)
    {
        Result<Notification> result = await _mediator
            .Send(new MarkNotificationReadCommand(notificationId), ct).ConfigureAwait(false);
        return result.ToHttpResult(n => Ok(Map(n)));
    }

    [HttpPost("{notificationId:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid notificationId, CancellationToken ct)
    {
        Result<Notification> result = await _mediator
            .Send(new DismissNotificationCommand(notificationId), ct).ConfigureAwait(false);
        return result.ToHttpResult(n => Ok(Map(n)));
    }

    private static NotificationResponse Map(Notification n) => new(
        n.Id.Value,
        n.Type.ToString(),
        n.Message,
        n.CreatedAt,
        n.Status.ToString(),
        n.ReadAt);
}

public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Message,
    DateTimeOffset CreatedAt,
    string Status,
    DateTimeOffset? ReadAt);
