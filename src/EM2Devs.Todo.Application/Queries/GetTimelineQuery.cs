using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetTimelineQuery(
    string? EventType = null,
    Guid? Cursor = null,
    int PageSize = 20) : IRequest<Result<TimelineReadModel>>;

public sealed class GetTimelineQueryHandler
    : IRequestHandler<GetTimelineQuery, Result<TimelineReadModel>>
{
    private readonly ITimelineRepository _repository;

    public GetTimelineQueryHandler(ITimelineRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TimelineReadModel>> Handle(GetTimelineQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<TimelineEvent> events = await _repository.GetEventsAsync(ct).ConfigureAwait(false);
        Timeline timeline = new(events);

        if (request.EventType is not null
            && Enum.TryParse<TimelineEventType>(request.EventType, ignoreCase: true, out var filter))
        {
            timeline = timeline.FilterByEventType(filter);
        }

        TimelinePage page;
        if (request.Cursor.HasValue)
        {
            try
            {
                page = timeline.GetNextPage(new TimelineEventId(request.Cursor.Value), request.PageSize);
            }
            catch (Domain.Exceptions.DomainException)
            {
                page = timeline.GetFirstPage(request.PageSize);
            }
        }
        else
        {
            page = timeline.GetFirstPage(request.PageSize);
        }

        var items = page.Events
            .Select(e => new TimelineEventReadModel(
                e.Id.Value,
                e.EventType.ToString(),
                e.OccurredAt,
                e.Details,
                e.Note?.Text))
            .ToList();

        return new TimelineReadModel(items, page.HasMore, page.NextCursor?.Value);
    }
}
