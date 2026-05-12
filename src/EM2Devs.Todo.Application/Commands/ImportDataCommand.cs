using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record ImportDataCommand(DataExportEnvelopeReadModel Envelope)
    : IRequest<Result<ImportResult>>;

public sealed record ImportResult(int RecordsImported);

/// <summary>
/// Restores the authenticated user's data from a snapshot in the same shape as the
/// JSON export envelope. Calls the purger first to wipe existing state, then walks
/// each section and reconstitutes entities via the Reconstitute factories on Domain.
/// </summary>
public sealed class ImportDataCommandHandler
    : IRequestHandler<ImportDataCommand, Result<ImportResult>>
{
    private readonly IUserDataPurger _purger;
    private readonly ITaskRepository _taskRepository;
    private readonly IQuestRepository _questRepository;
    private readonly IEpicRepository _epicRepository;
    private readonly IPlayerProfileRepository _profileRepository;
    private readonly IWeeklyReflectionRepository _weeklyReflectionRepository;
    private readonly IInsightCardRepository _insightCardRepository;
    private readonly ITimelineRepository _timelineRepository;
    private readonly ICurrentUser _currentUser;

    public ImportDataCommandHandler(
        IUserDataPurger purger,
        ITaskRepository taskRepository,
        IQuestRepository questRepository,
        IEpicRepository epicRepository,
        IPlayerProfileRepository profileRepository,
        IWeeklyReflectionRepository weeklyReflectionRepository,
        IInsightCardRepository insightCardRepository,
        ITimelineRepository timelineRepository,
        ICurrentUser currentUser)
    {
        _purger = purger;
        _taskRepository = taskRepository;
        _questRepository = questRepository;
        _epicRepository = epicRepository;
        _profileRepository = profileRepository;
        _weeklyReflectionRepository = weeklyReflectionRepository;
        _insightCardRepository = insightCardRepository;
        _timelineRepository = timelineRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<ImportResult>> Handle(ImportDataCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Envelope is null)
        {
            return Result<ImportResult>.Failure(new ValidationError("Import envelope is required."));
        }

        DataExportEnvelopeReadModel env = request.Envelope;
        Guid userId = _currentUser.UserId;

        // Wipe existing per-user data — the import is a wholesale replace.
        await _purger.PurgeAllForCurrentUserAsync(ct).ConfigureAwait(false);

        int imported = 0;

        // Quests first — tasks reference them via AssignedQuestId.
        foreach (QuestExportSnapshot q in env.Quests.Where(x => x is not null))
        {
            Quest quest = Quest.Reconstitute(
                new QuestId(q.Id),
                new QuestTitle(q.Title),
                q.Description,
                q.DueDate,
                q.IsCompleted,
                q.AssignedEpicId.HasValue ? new EpicId(q.AssignedEpicId.Value) : null,
                new ExperiencePoints(q.TotalXpEarned));
            await _questRepository.SaveAsync(quest, ct).ConfigureAwait(false);
            imported++;
        }

        foreach (EpicExportSnapshot e in env.Epics.Where(x => x is not null))
        {
            Epic epic = Epic.Reconstitute(
                new EpicId(e.Id),
                new EpicTitle(e.Title),
                e.Description,
                e.TargetDate,
                e.IsCompleted,
                sagaId: null);
            await _epicRepository.SaveAsync(epic, ct).ConfigureAwait(false);
            imported++;
        }

        foreach (TaskExportSnapshot t in env.Tasks.Where(x => x is not null))
        {
            TodoTask task = TodoTask.Reconstitute(
                new TaskId(t.Id),
                userId,
                new TaskTitle(t.Title),
                t.Description,
                Enum.Parse<Domain.TaskStatus>(t.Status),
                t.IsBossTask,
                Enum.Parse<TaskDifficulty>(t.Difficulty),
                Enum.Parse<TaskPriority>(t.Priority),
                t.EstimatedMinutes.HasValue ? TimeEstimate.FromMinutes(t.EstimatedMinutes.Value) : null,
                t.DueDate,
                t.CompletedAt,
                t.CreatedAt,
                sourceRecurringTaskId: null,
                t.ScheduledDate,
                t.AssignedQuestId.HasValue ? new QuestId(t.AssignedQuestId.Value) : null,
                actualTimeRecord: null,
                t.Tags.Select(Tag.From));
            await _taskRepository.SaveAsync(task, ct).ConfigureAwait(false);
            imported++;
        }

        foreach (WeeklyReflectionSnapshot r in env.WeeklyReviews.Where(x => x is not null))
        {
            await _weeklyReflectionRepository.SaveAsync(
                r.WeekOf, r.WhatWentWell, r.WhatDragged, r.Adjustment, r.SavedAt, ct)
                .ConfigureAwait(false);
            imported++;
        }

        foreach (InsightCardReadModel ic in env.InsightCards.Where(x => x is not null))
        {
            InsightCard card = InsightCard.Reconstitute(
                new InsightCardId(ic.Id),
                Enum.Parse<InsightType>(ic.Type),
                ic.Message,
                ic.SupportingData,
                Enum.Parse<InsightCardStatus>(ic.Status),
                ic.GeneratedAt,
                isValidated: true);
            await _insightCardRepository.AddAsync(card, ct).ConfigureAwait(false);
            imported++;
        }

        foreach (TimelineEventReadModel te in env.TimelineEvents.Where(x => x is not null))
        {
            TimelineEvent ev = TimelineEvent.Reconstitute(
                new TimelineEventId(te.Id),
                Enum.Parse<TimelineEventType>(te.EventType),
                te.OccurredAt,
                te.Details,
                te.Note is null ? null : new PersonalNote(te.Note, te.OccurredAt));
            await _timelineRepository.AddAsync(ev, ct).ConfigureAwait(false);
            imported++;
        }

        // PlayerProfile: rebuild from the envelope's Level / XpHistory / Titles / SkillTreeProgress.
        PlayerProfile profile = BuildProfile(env, userId);
        await _profileRepository.ImportAsync(profile, ct).ConfigureAwait(false);
        imported++;

        return new ImportResult(imported);
    }

    private static PlayerProfile BuildProfile(DataExportEnvelopeReadModel env, Guid userId)
    {
        Level level = new(env.Level.Current, new ExperiencePoints(env.Level.Xp));
        Streak streak = Streak.NewStreak();

        TitleInventory inventory = TitleInventory.Empty();
        foreach (TitleReadModel t in env.TitlesEarned.Where(x => x is not null))
        {
            if (Enum.TryParse<TitleType>(t.Type, out TitleType type))
            {
                inventory = inventory.AwardTitle(new Title(type, t.EarnedOn));
            }
        }

        XpHistory history = XpHistory.Empty();
        foreach (XpHistoryEntryReadModel entry in env.XpHistory.Where(x => x is not null))
        {
            history = history.RecordXpEarning(entry.Date, new ExperiencePoints(entry.XpEarned), entry.Source);
        }

        List<SkillTree> skillTrees = [];
        foreach (SkillTreeReadModel st in env.SkillTreeProgress.Where(x => x is not null))
        {
            if (Enum.TryParse<SkillTreeType>(st.Type, out SkillTreeType treeType))
            {
                SkillTier tier = new(st.Tier ?? 1);
                int completed = st.TasksCompletedInTier ?? 0;
                skillTrees.Add(new SkillTree(treeType, tier, completed));
            }
        }

        return PlayerProfile.Reconstitute(
            PlayerProfileId.New(),
            userId,
            level,
            streak,
            env.Level.LongestStreak,
            inventory,
            history,
            skillTrees);
    }
}
