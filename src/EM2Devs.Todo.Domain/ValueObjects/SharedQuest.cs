namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A shared quest that allows multiple participants to collaborate on tasks.
/// Manages participants, invitations, tasks, and progress.
/// </summary>
public sealed record SharedQuest
{
    public const int MaxParticipants = 10;

    /// <summary>XP bonus awarded to each participant when a shared quest is completed.</summary>
    public const int CompletionBonusXp = 100;

    private readonly List<SharedQuestParticipant> _participants;
    private readonly List<SharedQuestInvitation> _invitations;
    private readonly List<SharedQuestTask> _tasks;

    public SharedQuestId Id { get; }
    public string Title { get; }
    public string Description { get; }
    public DateOnly? DueDate { get; }
    public bool IsCompleted { get; }
    public IReadOnlyList<SharedQuestParticipant> Participants => _participants.AsReadOnly();
    public IReadOnlyList<SharedQuestInvitation> Invitations => _invitations.AsReadOnly();
    public IReadOnlyList<SharedQuestTask> Tasks => _tasks.AsReadOnly();
    public int CompletedTaskCount => _tasks.Count(t => t.IsCompleted);
    public int TotalTaskCount => _tasks.Count;

    public SharedQuest(SharedQuestId id, string title, string description, DateOnly? dueDate,
        IEnumerable<SharedQuestParticipant> participants,
        IEnumerable<SharedQuestInvitation>? invitations = null,
        IEnumerable<SharedQuestTask>? tasks = null,
        bool isCompleted = false)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new Exceptions.DomainException("Shared quest title cannot be empty.");
        }

        if (title.Length > 200)
        {
            throw new Exceptions.DomainException("Shared quest title cannot exceed 200 characters.");
        }

        _participants = participants?.ToList()
            ?? throw new ArgumentNullException(nameof(participants));

        if (_participants.Count > MaxParticipants)
        {
            throw new Exceptions.DomainException(
                $"Shared quest cannot have more than {MaxParticipants} participants.");
        }

        if (!isCompleted && !_participants.Exists(p => p.Role == SharedQuestRole.Creator))
        {
            throw new Exceptions.DomainException("Shared quest must have a creator.");
        }

        Title = title;
        Description = description ?? string.Empty;
        DueDate = dueDate;
        IsCompleted = isCompleted;
        _invitations = invitations?.ToList() ?? [];
        _tasks = tasks?.ToList() ?? [];
    }

    /// <summary>
    /// Create a new shared quest with a creator.
    /// </summary>
    public static SharedQuest Create(string title, string description, Guid creatorId, DateOnly today,
        DateOnly? dueDate = null)
    {
        if (creatorId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Creator ID cannot be empty.");
        }

        var creator = new SharedQuestParticipant(creatorId, SharedQuestRole.Creator, today);
        return new SharedQuest(SharedQuestId.New(), title, description, dueDate, [creator]);
    }

    /// <summary>
    /// The creator's user ID.
    /// </summary>
    public Guid CreatorId =>
        _participants.Find(p => p.Role == SharedQuestRole.Creator)!.UserId;

    /// <summary>
    /// Check if a user is a participant.
    /// </summary>
    public bool IsParticipant(Guid userId) =>
        _participants.Exists(p => p.UserId == userId);

    /// <summary>
    /// Invite a user to the shared quest.
    /// </summary>
    public SharedQuest InviteUser(Guid inviteeId)
    {
        if (_participants.Exists(p => p.UserId == inviteeId))
        {
            throw new Exceptions.DomainException("User is already a participant.");
        }

        if (_invitations.Exists(i => i.InviteeId == inviteeId && i.Status == SharedQuestInvitationStatus.Pending))
        {
            throw new Exceptions.DomainException("User already has a pending invitation.");
        }

        if (_participants.Count >= MaxParticipants)
        {
            throw new Exceptions.DomainException(
                $"Shared quest has reached its maximum of {MaxParticipants} participants.");
        }

        var invitation = new SharedQuestInvitation(
            SharedQuestInvitationId.New(), Id, inviteeId, SharedQuestInvitationStatus.Pending);
        List<SharedQuestInvitation> updatedInvitations = [.. _invitations, invitation];
        return new SharedQuest(Id, Title, Description, DueDate, _participants,
            updatedInvitations, _tasks, IsCompleted);
    }

    /// <summary>
    /// Accept an invitation and add the invitee as a participant.
    /// </summary>
    public SharedQuest AcceptInvitation(Guid inviteeId, DateOnly today)
    {
        SharedQuestInvitation? invitation = _invitations.Find(
            i => i.InviteeId == inviteeId && i.Status == SharedQuestInvitationStatus.Pending);

        if (invitation is null)
        {
            throw new Exceptions.DomainException("No pending invitation found for this user.");
        }

        if (_participants.Count >= MaxParticipants)
        {
            throw new Exceptions.DomainException(
                $"Shared quest has reached its maximum of {MaxParticipants} participants.");
        }

        SharedQuestInvitation accepted = invitation.Accept();
        List<SharedQuestInvitation> updatedInvitations = _invitations
            .Select(i => i.Id == invitation.Id ? accepted : i)
            .ToList();

        var newParticipant = new SharedQuestParticipant(inviteeId, SharedQuestRole.Participant, today);
        List<SharedQuestParticipant> updatedParticipants = [.. _participants, newParticipant];

        return new SharedQuest(Id, Title, Description, DueDate, updatedParticipants,
            updatedInvitations, _tasks, IsCompleted);
    }

    /// <summary>
    /// Decline an invitation.
    /// </summary>
    public SharedQuest DeclineInvitation(Guid inviteeId)
    {
        SharedQuestInvitation? invitation = _invitations.Find(
            i => i.InviteeId == inviteeId && i.Status == SharedQuestInvitationStatus.Pending);

        if (invitation is null)
        {
            throw new Exceptions.DomainException("No pending invitation found for this user.");
        }

        SharedQuestInvitation declined = invitation.Decline();
        List<SharedQuestInvitation> updatedInvitations = _invitations
            .Select(i => i.Id == invitation.Id ? declined : i)
            .ToList();

        return new SharedQuest(Id, Title, Description, DueDate, _participants,
            updatedInvitations, _tasks, IsCompleted);
    }

    /// <summary>
    /// Add a task to the shared quest. Only participants can add tasks.
    /// </summary>
    public SharedQuest AddTask(string title, Guid addedByUserId, Guid? assigneeUserId = null)
    {
        if (!IsParticipant(addedByUserId))
        {
            throw new Exceptions.DomainException("Only participants can add tasks to a shared quest.");
        }

        if (assigneeUserId.HasValue && !IsParticipant(assigneeUserId.Value))
        {
            throw new Exceptions.DomainException("Tasks can only be assigned to participants.");
        }

        var task = new SharedQuestTask(SharedQuestTaskId.New(), title, assigneeUserId);
        List<SharedQuestTask> updatedTasks = [.. _tasks, task];

        return new SharedQuest(Id, Title, Description, DueDate, _participants,
            _invitations, updatedTasks, IsCompleted);
    }

    /// <summary>
    /// Complete a task in the shared quest. Returns a tuple of (updatedQuest, questJustCompleted).
    /// When all tasks are done, the quest is automatically completed.
    /// </summary>
    public (SharedQuest Quest, bool JustCompleted) CompleteTask(SharedQuestTaskId taskId)
    {
        ArgumentNullException.ThrowIfNull(taskId);

        SharedQuestTask? task = _tasks.Find(t => t.Id == taskId);
        if (task is null)
        {
            throw new Exceptions.DomainException("Task not found in this shared quest.");
        }

        List<SharedQuestTask> updatedTasks = _tasks
            .Select(t => t.Id == taskId ? t.Complete() : t)
            .ToList();

        bool allComplete = updatedTasks.All(t => t.IsCompleted);
        bool justCompleted = allComplete && !IsCompleted;

        SharedQuest updated = new(Id, Title, Description, DueDate, _participants,
            _invitations, updatedTasks, allComplete);

        return (updated, justCompleted);
    }

    /// <summary>
    /// A participant leaves the quest. Their incomplete tasks become unassigned.
    /// The creator cannot leave.
    /// </summary>
    public SharedQuest Leave(Guid userId)
    {
        SharedQuestParticipant? participant = _participants.Find(p => p.UserId == userId);

        if (participant is null)
        {
            throw new Exceptions.DomainException("User is not a participant in this shared quest.");
        }

        if (participant.Role == SharedQuestRole.Creator)
        {
            throw new Exceptions.DomainException("The creator cannot leave the shared quest.");
        }

        List<SharedQuestParticipant> updatedParticipants = _participants
            .Where(p => p.UserId != userId).ToList();

        List<SharedQuestTask> updatedTasks = _tasks
            .Select(t => t.AssigneeUserId == userId && !t.IsCompleted ? t.Unassign() : t)
            .ToList();

        return new SharedQuest(Id, Title, Description, DueDate, updatedParticipants,
            _invitations, updatedTasks, IsCompleted);
    }

    /// <summary>
    /// Creator removes a participant. Their incomplete tasks become unassigned.
    /// Only the creator can remove participants.
    /// </summary>
    public SharedQuest RemoveParticipant(Guid requesterId, Guid participantId)
    {
        SharedQuestParticipant? requester = _participants.Find(p => p.UserId == requesterId);

        if (requester is null || requester.Role != SharedQuestRole.Creator)
        {
            throw new Exceptions.DomainException("Only the creator can remove participants.");
        }

        if (requesterId == participantId)
        {
            throw new Exceptions.DomainException("The creator cannot remove themselves.");
        }

        SharedQuestParticipant? participant = _participants.Find(p => p.UserId == participantId);

        if (participant is null)
        {
            throw new Exceptions.DomainException("User is not a participant in this shared quest.");
        }

        List<SharedQuestParticipant> updatedParticipants = _participants
            .Where(p => p.UserId != participantId).ToList();

        List<SharedQuestTask> updatedTasks = _tasks
            .Select(t => t.AssigneeUserId == participantId && !t.IsCompleted ? t.Unassign() : t)
            .ToList();

        return new SharedQuest(Id, Title, Description, DueDate, updatedParticipants,
            _invitations, updatedTasks, IsCompleted);
    }

    /// <summary>
    /// Update quest details. Only the creator can do this.
    /// </summary>
    public SharedQuest UpdateDetails(Guid requesterId, string newTitle, string newDescription, DateOnly? newDueDate)
    {
        SharedQuestParticipant? requester = _participants.Find(p => p.UserId == requesterId);

        if (requester is null || requester.Role != SharedQuestRole.Creator)
        {
            throw new Exceptions.DomainException("Only the creator can edit shared quest details.");
        }

        return new SharedQuest(Id, newTitle, newDescription, newDueDate, _participants,
            _invitations, _tasks, IsCompleted);
    }

    /// <summary>
    /// Returns the progress as a percentage (0 to 100). Zero if no tasks.
    /// </summary>
    public int Progress => TotalTaskCount == 0 ? 0 : CompletedTaskCount * 100 / TotalTaskCount;

    /// <summary>
    /// Get the contribution breakdown per participant.
    /// Returns a list of (UserId, CompletedCount, TotalAssigned).
    /// </summary>
    public IReadOnlyList<(Guid UserId, int CompletedCount, int TotalAssigned)> GetContributions()
    {
        return _participants.Select(p =>
        {
            List<SharedQuestTask> userTasks = _tasks
                .Where(t => t.AssigneeUserId == p.UserId).ToList();
            int completed = userTasks.Count(t => t.IsCompleted);
            return (p.UserId, completed, userTasks.Count);
        }).ToList().AsReadOnly();
    }

    /// <summary>
    /// Get tasks by their completion status.
    /// </summary>
    public (IReadOnlyList<SharedQuestTask> Completed, IReadOnlyList<SharedQuestTask> Pending) GetTasksByStatus()
    {
        List<SharedQuestTask> completed = _tasks.Where(t => t.IsCompleted).ToList();
        List<SharedQuestTask> pending = _tasks.Where(t => !t.IsCompleted).ToList();
        return (completed.AsReadOnly(), pending.AsReadOnly());
    }
}
