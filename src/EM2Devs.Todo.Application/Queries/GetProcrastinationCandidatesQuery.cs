using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetProcrastinationCandidatesQuery : IRequest<Result<IReadOnlyList<ProcrastinationCandidateReadModel>>>;

public sealed class GetProcrastinationCandidatesQueryHandler
    : IRequestHandler<GetProcrastinationCandidatesQuery, Result<IReadOnlyList<ProcrastinationCandidateReadModel>>>
{
    private readonly ITaskRepository _taskRepository;

    public GetProcrastinationCandidatesQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<IReadOnlyList<ProcrastinationCandidateReadModel>>> Handle(
        GetProcrastinationCandidatesQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);

        var candidates = new List<ProcrastinationCandidateReadModel>();
        foreach (TodoTask task in tasks)
        {
            ProcrastinationCandidate? candidate = ProcrastinationEvaluator.Evaluate(task);
            if (candidate is not null)
            {
                candidates.Add(new ProcrastinationCandidateReadModel(
                    candidate.TaskId.Value,
                    task.Title.Value,
                    candidate.UrgencyScore,
                    candidate.Signals.Select(s => s.Type.ToString()).ToList(),
                    candidate.AvailableInterventions
                        .Select(i => new InterventionOptionReadModel(i.Type.ToString(), i.SupportiveMessage))
                        .ToList()));
            }
        }

        candidates.Sort((a, b) => b.UrgencyScore.CompareTo(a.UrgencyScore));

        return candidates;
    }
}
