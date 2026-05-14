using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record ExportDataQuery(DataExportFormat Format, DataExportScope Scope)
    : IRequest<Result<DataExportEnvelopeReadModel>>;

public sealed class ExportDataQueryHandler
    : IRequestHandler<ExportDataQuery, Result<DataExportEnvelopeReadModel>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IQuestRepository _questRepository;
    private readonly IEpicRepository _epicRepository;
    private readonly IPlayerProfileRepository _profileRepository;
    private readonly IWeeklyReflectionRepository _weeklyReflectionRepository;
    private readonly IInsightCardRepository _insightCardRepository;
    private readonly ITimelineRepository _timelineRepository;
    private readonly TimeProvider _timeProvider;

    public ExportDataQueryHandler(
        ITaskRepository taskRepository,
        IQuestRepository questRepository,
        IEpicRepository epicRepository,
        IPlayerProfileRepository profileRepository,
        IWeeklyReflectionRepository weeklyReflectionRepository,
        IInsightCardRepository insightCardRepository,
        ITimelineRepository timelineRepository,
        TimeProvider timeProvider)
    {
        _taskRepository = taskRepository;
        _questRepository = questRepository;
        _epicRepository = epicRepository;
        _profileRepository = profileRepository;
        _weeklyReflectionRepository = weeklyReflectionRepository;
        _insightCardRepository = insightCardRepository;
        _timelineRepository = timelineRepository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<DataExportEnvelopeReadModel>> Handle(ExportDataQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Format != DataExportFormat.Json || request.Scope != DataExportScope.All)
        {
            return Result<DataExportEnvelopeReadModel>.Failure(
                new ValidationError("Only format=json with scope=all is currently supported."));
        }

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);
        IReadOnlyList<Quest> quests = await _questRepository.GetAllAsync(ct).ConfigureAwait(false);
        IReadOnlyList<Epic> epics = await _epicRepository.GetAllAsync(ct).ConfigureAwait(false);
        PlayerProfileReadModel profile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);
        IReadOnlyList<WeeklyReflectionSnapshot> reflections = await _weeklyReflectionRepository
            .ListAllForCurrentUserAsync(ct).ConfigureAwait(false);
        IReadOnlyList<InsightCard> insights = await _insightCardRepository
            .GetForCurrentUserAsync(includeRead: true, ct).ConfigureAwait(false);
        IReadOnlyList<TimelineEvent> timelineEvents = await _timelineRepository
            .GetEventsAsync(ct).ConfigureAwait(false);

        List<TaskExportSnapshot> taskSnapshots = tasks.Select(MapTask).ToList();
        List<QuestExportSnapshot> questSnapshots = quests.Select(MapQuest).ToList();
        List<EpicExportSnapshot> epicSnapshots = epics.Select(MapEpic).ToList();
        List<TimelineEventReadModel> timelineSnapshots = timelineEvents.Select(MapTimeline).ToList();
        List<InsightCardReadModel> insightSnapshots = insights.Select(MapInsight).ToList();
        IReadOnlyList<XpHistoryEntryReadModel> xpHistory = profile.XpHistory ?? [];
        IReadOnlyList<SkillTreeReadModel> skillTrees = profile.SkillTrees ?? [];
        IReadOnlyList<TitleReadModel> titlesEarned = profile.Titles?.Earned ?? [];
        IReadOnlyList<System.Text.Json.JsonElement> sagas = [];

        DataExportLevelSection level = new(profile.Level, profile.TotalXp, profile.LongestStreak);
        DataExportSettingsSection settings = new(
            DataPrivacy: null,
            Notifications: null,
            Sync: null,
            Leaderboard: null);

        int recordCount = taskSnapshots.Count
            + questSnapshots.Count
            + epicSnapshots.Count
            + sagas.Count
            + xpHistory.Count
            + skillTrees.Count
            + titlesEarned.Count
            + reflections.Count
            + timelineSnapshots.Count
            + insightSnapshots.Count;

        DataExportMetaSection meta = new(
            ExportedAt: _timeProvider.GetUtcNow(),
            Format: "json",
            Scope: "all",
            RecordCount: recordCount);

        DataExportEnvelopeReadModel envelope = new(
            Meta: meta,
            Tasks: taskSnapshots,
            Quests: questSnapshots,
            Epics: epicSnapshots,
            Sagas: sagas,
            XpHistory: xpHistory,
            Level: level,
            SkillTreeProgress: skillTrees,
            TitlesEarned: titlesEarned,
            WeeklyReviews: reflections,
            TimelineEvents: timelineSnapshots,
            InsightCards: insightSnapshots,
            Settings: settings);

        return envelope;
    }

    private static TaskExportSnapshot MapTask(TodoTask task) => new(
        Id: task.Id.Value,
        Title: task.Title.Value,
        Description: task.Description,
        Status: task.Status.ToString(),
        Difficulty: task.Difficulty.ToString(),
        Priority: task.Priority.ToString(),
        IsBossTask: task.IsBossTask,
        EstimatedMinutes: task.EstimatedTime?.Minutes,
        ActualMinutes: task.ActualTimeRecord?.Actual.Minutes,
        DueDate: task.DueDate,
        ScheduledDate: task.ScheduledDate,
        CreatedAt: task.CreatedAt,
        CompletedAt: task.CompletedAt,
        AssignedQuestId: task.AssignedQuestId?.Value,
        Tags: task.Tags.Select(t => t.Value).ToArray());

    private static QuestExportSnapshot MapQuest(Quest quest) => new(
        Id: quest.Id.Value,
        Title: quest.Title.Value,
        Description: quest.Description,
        DueDate: quest.DueDate,
        IsCompleted: quest.IsCompleted,
        TotalXpEarned: quest.TotalXpEarned.Value,
        AssignedEpicId: quest.EpicId?.Value,
        TaskIds: quest.Tasks.Select(t => t.Id.Value).ToArray());

    private static EpicExportSnapshot MapEpic(Epic epic) => new(
        Id: epic.Id.Value,
        Title: epic.Title.Value,
        Description: epic.Description,
        TargetDate: epic.TargetDate,
        IsCompleted: epic.IsCompleted,
        QuestIds: epic.Quests.Select(q => q.Id.Value).ToArray());

    private static TimelineEventReadModel MapTimeline(TimelineEvent e) => new(
        e.Id.Value,
        e.EventType.ToString(),
        e.OccurredAt,
        e.Details,
        e.Note?.Text);

    private static InsightCardReadModel MapInsight(InsightCard card) => new(
        card.Id.Value,
        card.Type.ToString(),
        card.Message,
        card.SupportingData,
        card.Status.ToString(),
        card.GeneratedAt);
}
