using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetSubscriptionQuery : IRequest<Result<SubscriptionReadModel>>;

public sealed class GetSubscriptionQueryHandler
    : IRequestHandler<GetSubscriptionQuery, Result<SubscriptionReadModel>>
{
    private readonly ICurrentUser _currentUser;

    public GetSubscriptionQueryHandler(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<Result<SubscriptionReadModel>> Handle(GetSubscriptionQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Subscription subscription = Subscription.CreateFree(_currentUser.UserId, DateTimeOffset.UtcNow);

        Result<SubscriptionReadModel> result = new SubscriptionReadModel(
            subscription.Tier.ToString(),
            subscription.Status.ToString(),
            subscription.IsPremium,
            subscription.IsActive,
            subscription.ExpiresAt,
            subscription.AutoRenew);

        return Task.FromResult(result);
    }
}
