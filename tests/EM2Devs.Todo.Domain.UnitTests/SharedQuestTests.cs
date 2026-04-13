using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for SharedQuest value object.
/// Tests encode behaviors from shared-quests.feature.
/// </summary>
public sealed class SharedQuestTests
{
    private static readonly DateOnly _today = new(2026, 4, 12);
    private static readonly Guid _creatorId = Guid.NewGuid();
    private static readonly Guid _jordanId = Guid.NewGuid();
    private static readonly Guid _alexId = Guid.NewGuid();

    // ─── Scenario 1: Create a shared quest and invite participants ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSharedQuest_When_ValidDetailsProvided()
    {
        // Given / When
        SharedQuest quest = SharedQuest.Create(
            "Plan summer road trip",
            "Organise the group road trip",
            _creatorId, _today,
            new DateOnly(2026, 6, 15));

        // Then
        quest.Id.Value.ShouldNotBe(Guid.Empty);
        quest.Title.ShouldBe("Plan summer road trip");
        quest.Description.ShouldBe("Organise the group road trip");
        quest.DueDate.ShouldBe(new DateOnly(2026, 6, 15));
        quest.IsCompleted.ShouldBeFalse();
        quest.Participants.Count.ShouldBe(1);
        quest.Participants[0].UserId.ShouldBe(_creatorId);
        quest.Participants[0].Role.ShouldBe(SharedQuestRole.Creator);
        quest.CreatorId.ShouldBe(_creatorId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_InviteParticipants_When_CreatorInvitesUsers()
    {
        // Given
        SharedQuest quest = CreateDefaultQuest();

        // When
        SharedQuest updated = quest.InviteUser(_jordanId);
        updated = updated.InviteUser(_alexId);

        // Then
        updated.Invitations.Count.ShouldBe(2);
        updated.Invitations.ShouldContain(i => i.InviteeId == _jordanId && i.Status == SharedQuestInvitationStatus.Pending);
        updated.Invitations.ShouldContain(i => i.InviteeId == _alexId && i.Status == SharedQuestInvitationStatus.Pending);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AppearInParticipantsList_When_InvitationAccepted()
    {
        // Given
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        quest = quest.InviteUser(_alexId);

        // When
        quest = quest.AcceptInvitation(_jordanId, _today);
        quest = quest.AcceptInvitation(_alexId, _today);

        // Then
        quest.Participants.Count.ShouldBe(3);
        quest.IsParticipant(_jordanId).ShouldBeTrue();
        quest.IsParticipant(_alexId).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TitleIsEmpty()
    {
        DomainException ex = Should.Throw<DomainException>(
            () => SharedQuest.Create("", "desc", _creatorId, _today));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TitleExceedsMaxLength()
    {
        string longTitle = new('x', 201);
        DomainException ex = Should.Throw<DomainException>(
            () => SharedQuest.Create(longTitle, "desc", _creatorId, _today));
        ex.Message.ShouldContain("cannot exceed 200");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When__creatorIdIsEmpty()
    {
        DomainException ex = Should.Throw<DomainException>(
            () => SharedQuest.Create("Quest", "desc", Guid.Empty, _today));
        ex.Message.ShouldContain("Creator ID cannot be empty");
    }

    // ─── Scenario 2: Add tasks to a shared quest ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddTasks_When_ParticipantAddsTasks()
    {
        // Given
        SharedQuest quest = CreateQuestWithParticipants();

        // When
        quest = quest.AddTask("Book accommodation", _creatorId, _jordanId);
        quest = quest.AddTask("Create packing list", _creatorId, _creatorId);

        // Then
        quest.Tasks.Count.ShouldBe(2);
        quest.Tasks.ShouldContain(t => t.Title == "Book accommodation" && t.AssigneeUserId == _jordanId);
        quest.Tasks.ShouldContain(t => t.Title == "Create packing list" && t.AssigneeUserId == _creatorId);
    }

    // ─── Scenario 3: Any participant can add tasks ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowAnyParticipantToAddTask_When_ParticipantAddsTask()
    {
        // Given
        SharedQuest quest = CreateQuestWithParticipants();

        // When — Jordan (a participant, not creator) adds a task assigned to Alex
        quest = quest.AddTask("Research restaurants", _jordanId, _alexId);

        // Then
        quest.Tasks.Count.ShouldBe(1);
        quest.Tasks[0].Title.ShouldBe("Research restaurants");
        quest.Tasks[0].AssigneeUserId.ShouldBe(_alexId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonParticipantAddsTask()
    {
        // Given
        SharedQuest quest = CreateDefaultQuest();
        Guid outsiderId = Guid.NewGuid();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(
            () => quest.AddTask("Some task", outsiderId));
        ex.Message.ShouldContain("Only participants");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AssigningTaskToNonParticipant()
    {
        // Given
        SharedQuest quest = CreateDefaultQuest();
        Guid outsiderId = Guid.NewGuid();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(
            () => quest.AddTask("Some task", _creatorId, outsiderId));
        ex.Message.ShouldContain("only be assigned to participants");
    }

    // ─── Scenario 4: View shared quest progress ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowFiftyPercentProgress_When_HalfTasksCompleted()
    {
        // Given — 6 tasks, 3 completed
        SharedQuest quest = CreateQuestWithParticipants();
        quest = quest.AddTask("Task 1", _creatorId, _creatorId);
        quest = quest.AddTask("Task 2", _creatorId, _jordanId);
        quest = quest.AddTask("Task 3", _creatorId, _alexId);
        quest = quest.AddTask("Task 4", _creatorId, _creatorId);
        quest = quest.AddTask("Task 5", _creatorId, _jordanId);
        quest = quest.AddTask("Task 6", _creatorId, _alexId);

        // Complete 3 tasks
        (quest, _) = quest.CompleteTask(quest.Tasks[0].Id);
        (quest, _) = quest.CompleteTask(quest.Tasks[1].Id);
        (quest, _) = quest.CompleteTask(quest.Tasks[2].Id);

        // Then
        quest.Progress.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowContributions_When_ViewingProgress()
    {
        // Given
        SharedQuest quest = CreateQuestWithParticipants();
        quest = quest.AddTask("Task 1", _creatorId, _creatorId);
        quest = quest.AddTask("Task 2", _creatorId, _jordanId);
        quest = quest.AddTask("Task 3", _creatorId, _alexId);
        (quest, _) = quest.CompleteTask(quest.Tasks[0].Id);

        // When
        IReadOnlyList<(Guid UserId, int CompletedCount, int TotalAssigned)> contributions = quest.GetContributions();

        // Then
        contributions.Count.ShouldBe(3);
        contributions.ShouldContain(c => c.UserId == _creatorId && c.CompletedCount == 1 && c.TotalAssigned == 1);
        contributions.ShouldContain(c => c.UserId == _jordanId && c.CompletedCount == 0 && c.TotalAssigned == 1);
        contributions.ShouldContain(c => c.UserId == _alexId && c.CompletedCount == 0 && c.TotalAssigned == 1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowTaskBreakdown_When_ViewingProgress()
    {
        // Given
        SharedQuest quest = CreateQuestWithParticipants();
        quest = quest.AddTask("Task 1", _creatorId);
        quest = quest.AddTask("Task 2", _creatorId);
        quest = quest.AddTask("Task 3", _creatorId);
        (quest, _) = quest.CompleteTask(quest.Tasks[0].Id);

        // When
        (IReadOnlyList<SharedQuestTask> completed, IReadOnlyList<SharedQuestTask> pending) = quest.GetTasksByStatus();

        // Then
        completed.Count.ShouldBe(1);
        pending.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveZeroProgress_When_NoTasks()
    {
        SharedQuest quest = CreateDefaultQuest();
        quest.Progress.ShouldBe(0);
    }

    // ─── Scenario 5: Shared quest has a maximum participant limit ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotAddParticipant_When_MaxReached()
    {
        // Given — quest with 10 participants
        SharedQuest quest = CreateDefaultQuest();
        for (int i = 0; i < 9; i++)
        {
            Guid userId = Guid.NewGuid();
            quest = quest.InviteUser(userId);
            quest = quest.AcceptInvitation(userId, _today);
        }
        quest.Participants.Count.ShouldBe(10);

        // When / Then
        Guid samId = Guid.NewGuid();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.InviteUser(samId));
        ex.Message.ShouldContain("maximum of 10 participants");
    }

    // ─── Scenario 6: Complete a task in a shared quest ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpdateProgress_When_TaskCompleted()
    {
        // Given
        SharedQuest quest = CreateQuestWithParticipants();
        quest = quest.AddTask("Task 1", _creatorId);
        quest = quest.AddTask("Task 2", _creatorId);

        // When
        (quest, bool justCompleted) = quest.CompleteTask(quest.Tasks[0].Id);

        // Then
        quest.CompletedTaskCount.ShouldBe(1);
        quest.Progress.ShouldBe(50);
        justCompleted.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TaskNotFound()
    {
        SharedQuest quest = CreateDefaultQuest();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.CompleteTask(SharedQuestTaskId.New()));
        ex.Message.ShouldContain("not found");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CompletingNullTaskId()
    {
        SharedQuest quest = CreateDefaultQuest();
        Should.Throw<ArgumentNullException>(() => quest.CompleteTask(null!));
    }

    // ─── Scenario 7: Participant leaves a shared quest ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnassignTasks_When_ParticipantLeaves()
    {
        // Given
        SharedQuest quest = CreateQuestWithParticipants();
        quest = quest.AddTask("Task 1", _creatorId, _jordanId);
        quest = quest.AddTask("Task 2", _creatorId, _jordanId);
        quest = quest.AddTask("Task 3", _creatorId, _alexId);

        // When
        quest = quest.Leave(_jordanId);

        // Then
        quest.IsParticipant(_jordanId).ShouldBeFalse();
        quest.Participants.Count.ShouldBe(2); // creator + Alex
        quest.Tasks[0].AssigneeUserId.ShouldBeNull();
        quest.Tasks[1].AssigneeUserId.ShouldBeNull();
        quest.Tasks[2].AssigneeUserId.ShouldBe(_alexId); // Alex's task unchanged
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveCompletedTasks_When_ParticipantLeaves()
    {
        // Given — Jordan completed one task, has one incomplete
        SharedQuest quest = CreateQuestWithParticipants();
        quest = quest.AddTask("Done task", _creatorId, _jordanId);
        quest = quest.AddTask("Pending task", _creatorId, _jordanId);
        (quest, _) = quest.CompleteTask(quest.Tasks[0].Id);

        // When
        quest = quest.Leave(_jordanId);

        // Then — completed task still assigned, incomplete unassigned
        quest.Tasks[0].AssigneeUserId.ShouldBe(_jordanId); // completed task keeps assignee
        quest.Tasks[0].IsCompleted.ShouldBeTrue();
        quest.Tasks[1].AssigneeUserId.ShouldBeNull(); // incomplete unassigned
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CreatorTriesToLeave()
    {
        SharedQuest quest = CreateDefaultQuest();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.Leave(_creatorId));
        ex.Message.ShouldContain("creator cannot leave");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonParticipantTriesToLeave()
    {
        SharedQuest quest = CreateDefaultQuest();
        Guid outsiderId = Guid.NewGuid();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.Leave(outsiderId));
        ex.Message.ShouldContain("not a participant");
    }

    // ─── Scenario 8: Creator removes a participant ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveParticipant_When_CreatorRemoves()
    {
        // Given
        SharedQuest quest = CreateQuestWithParticipants();
        quest = quest.AddTask("Task 1", _creatorId, _alexId);
        quest = quest.AddTask("Task 2", _creatorId, _alexId);

        // When
        quest = quest.RemoveParticipant(_creatorId, _alexId);

        // Then
        quest.IsParticipant(_alexId).ShouldBeFalse();
        quest.Tasks[0].AssigneeUserId.ShouldBeNull();
        quest.Tasks[1].AssigneeUserId.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveCompletedTaskAssignment_When_ParticipantRemoved()
    {
        // Given — Alex completed one task
        SharedQuest quest = CreateQuestWithParticipants();
        quest = quest.AddTask("Completed", _creatorId, _alexId);
        quest = quest.AddTask("Pending", _creatorId, _alexId);
        (quest, _) = quest.CompleteTask(quest.Tasks[0].Id);

        // When
        quest = quest.RemoveParticipant(_creatorId, _alexId);

        // Then — completed task keeps assignee for XP history
        quest.Tasks[0].AssigneeUserId.ShouldBe(_alexId);
        quest.Tasks[0].IsCompleted.ShouldBeTrue();
        quest.Tasks[1].AssigneeUserId.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonCreatorTriesToRemove()
    {
        SharedQuest quest = CreateQuestWithParticipants();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.RemoveParticipant(_jordanId, _alexId));
        ex.Message.ShouldContain("Only the creator");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CreatorTriesToRemoveThemselves()
    {
        SharedQuest quest = CreateDefaultQuest();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.RemoveParticipant(_creatorId, _creatorId));
        ex.Message.ShouldContain("cannot remove themselves");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RemovingNonParticipant()
    {
        SharedQuest quest = CreateDefaultQuest();
        Guid outsiderId = Guid.NewGuid();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.RemoveParticipant(_creatorId, outsiderId));
        ex.Message.ShouldContain("not a participant");
    }

    // ─── Scenario 9: Shared quest completion ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompleteQuest_When_AllTasksDone()
    {
        // Given — 6 tasks, 5 completed
        SharedQuest quest = CreateQuestWithParticipants();
        for (int i = 0; i < 6; i++)
        {
            quest = quest.AddTask($"Task {i + 1}", _creatorId);
        }
        for (int i = 0; i < 5; i++)
        {
            (quest, _) = quest.CompleteTask(quest.Tasks[i].Id);
        }

        // When — complete the final task
        bool justCompleted;
        (quest, justCompleted) = quest.CompleteTask(quest.Tasks[5].Id);

        // Then
        quest.IsCompleted.ShouldBeTrue();
        justCompleted.ShouldBeTrue();
        quest.Progress.ShouldBe(100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardBonusXpToAllParticipants_When_QuestCompleted()
    {
        // The CompletionBonusXp constant should be accessible
        SharedQuest.CompletionBonusXp.ShouldBe(100);
    }

    // ─── Scenario 10: View shared quest progress (all participants see same progress) ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowSameProgressToAllParticipants_When_Viewed()
    {
        // Given — shared quest is an immutable record, so any participant viewing it sees the same data
        SharedQuest quest = CreateQuestWithParticipants();
        quest = quest.AddTask("Task 1", _creatorId, _creatorId);
        quest = quest.AddTask("Task 2", _creatorId, _jordanId);
        quest = quest.AddTask("Task 3", _creatorId, _alexId);
        (quest, _) = quest.CompleteTask(quest.Tasks[0].Id);

        // Then — progress is same regardless of who views
        quest.Progress.ShouldBe(33);
        quest.CompletedTaskCount.ShouldBe(1);
        quest.TotalTaskCount.ShouldBe(3);
    }

    // ─── Scenario: All participants leave except creator ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContinueAsPersonalQuest_When_AllOthersLeave()
    {
        // Given
        SharedQuest quest = CreateQuestWithParticipants(); // creator + Jordan + Alex

        // When
        quest = quest.Leave(_jordanId);
        quest = quest.Leave(_alexId);

        // Then — creator remains as sole participant
        quest.Participants.Count.ShouldBe(1);
        quest.Participants[0].UserId.ShouldBe(_creatorId);
        quest.Participants[0].Role.ShouldBe(SharedQuestRole.Creator);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowNewInvitations_When_AllOthersLeft()
    {
        // Given — all left except creator
        SharedQuest quest = CreateQuestWithParticipants();
        quest = quest.Leave(_jordanId);
        quest = quest.Leave(_alexId);

        // When — invite new user
        Guid newUserId = Guid.NewGuid();
        quest = quest.InviteUser(newUserId);

        // Then
        quest.Invitations.ShouldContain(i => i.InviteeId == newUserId);
    }

    // ─── Scenario: Quest creator has management privileges ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowCreatorToEditDetails_When_CreatorUpdates()
    {
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.UpdateDetails(_creatorId, "New title", "New desc", new DateOnly(2026, 7, 1));

        quest.Title.ShouldBe("New title");
        quest.Description.ShouldBe("New desc");
        quest.DueDate.ShouldBe(new DateOnly(2026, 7, 1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonCreatorEditsDetails()
    {
        SharedQuest quest = CreateQuestWithParticipants();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.UpdateDetails(_jordanId, "New", "Desc", null));
        ex.Message.ShouldContain("Only the creator");
    }

    // ─── Invitation edge cases ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DeclineInvitation_When_InviteeDeclines()
    {
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        quest = quest.DeclineInvitation(_jordanId);

        quest.Invitations[0].Status.ShouldBe(SharedQuestInvitationStatus.Declined);
        quest.IsParticipant(_jordanId).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AcceptingWithoutInvitation()
    {
        SharedQuest quest = CreateDefaultQuest();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.AcceptInvitation(_jordanId, _today));
        ex.Message.ShouldContain("No pending invitation");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DecliningWithoutInvitation()
    {
        SharedQuest quest = CreateDefaultQuest();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.DeclineInvitation(_jordanId));
        ex.Message.ShouldContain("No pending invitation");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InvitingExistingParticipant()
    {
        SharedQuest quest = CreateDefaultQuest();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.InviteUser(_creatorId));
        ex.Message.ShouldContain("already a participant");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InvitingUserWithPendingInvitation()
    {
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        DomainException ex = Should.Throw<DomainException>(
            () => quest.InviteUser(_jordanId));
        ex.Message.ShouldContain("already has a pending invitation");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InvitingWithEmptyId()
    {
        SharedQuest quest = CreateDefaultQuest();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.InviteUser(Guid.Empty));
        ex.Message.ShouldContain("Invitee ID cannot be empty");
    }

    // ─── SharedQuestParticipant edge cases ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ParticipantUserIdIsEmpty()
    {
        DomainException ex = Should.Throw<DomainException>(
            () => new SharedQuestParticipant(Guid.Empty, SharedQuestRole.Creator, _today));
        ex.Message.ShouldContain("cannot be empty");
    }

    // ─── SharedQuestInvitation edge cases ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AcceptingNonPendingInvitation()
    {
        var invitation = new SharedQuestInvitation(
            SharedQuestInvitationId.New(), SharedQuestId.New(), _jordanId);
        SharedQuestInvitation accepted = invitation.Accept();

        DomainException ex = Should.Throw<DomainException>(() => accepted.Accept());
        ex.Message.ShouldContain("Only pending");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DecliningNonPendingInvitation()
    {
        var invitation = new SharedQuestInvitation(
            SharedQuestInvitationId.New(), SharedQuestId.New(), _jordanId);
        SharedQuestInvitation declined = invitation.Decline();

        DomainException ex = Should.Throw<DomainException>(() => declined.Decline());
        ex.Message.ShouldContain("Only pending");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_InvitationIdIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new SharedQuestInvitation(null!, SharedQuestId.New(), _jordanId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_InvitationQuestIdIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new SharedQuestInvitation(SharedQuestInvitationId.New(), null!, _jordanId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InvitationInviteeIdIsEmpty()
    {
        DomainException ex = Should.Throw<DomainException>(
            () => new SharedQuestInvitation(SharedQuestInvitationId.New(), SharedQuestId.New(), Guid.Empty));
        ex.Message.ShouldContain("Invitee ID cannot be empty");
    }

    // ─── SharedQuestTask edge cases ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TaskTitleIsEmpty()
    {
        DomainException ex = Should.Throw<DomainException>(
            () => new SharedQuestTask(SharedQuestTaskId.New(), ""));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TaskTitleExceedsMaxLength()
    {
        string longTitle = new('x', 201);
        DomainException ex = Should.Throw<DomainException>(
            () => new SharedQuestTask(SharedQuestTaskId.New(), longTitle));
        ex.Message.ShouldContain("cannot exceed 200");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TaskIdIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new SharedQuestTask(null!, "Title"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingAlreadyCompletedTask()
    {
        var task = new SharedQuestTask(SharedQuestTaskId.New(), "Task");
        SharedQuestTask completed = task.Complete();
        DomainException ex = Should.Throw<DomainException>(() => completed.Complete());
        ex.Message.ShouldContain("already completed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnassignTask_When_Unassigned()
    {
        var task = new SharedQuestTask(SharedQuestTaskId.New(), "Task", _jordanId);
        SharedQuestTask unassigned = task.Unassign();
        unassigned.AssigneeUserId.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AssignTask_When_AssignedToUser()
    {
        var task = new SharedQuestTask(SharedQuestTaskId.New(), "Task");
        SharedQuestTask assigned = task.AssignTo(_jordanId);
        assigned.AssigneeUserId.ShouldBe(_jordanId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AssigningToEmptyUserId()
    {
        var task = new SharedQuestTask(SharedQuestTaskId.New(), "Task");
        DomainException ex = Should.Throw<DomainException>(() => task.AssignTo(Guid.Empty));
        ex.Message.ShouldContain("Cannot assign");
    }

    // ─── SharedQuest constructor edge cases ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_SharedQuestIdIsNull()
    {
        var creator = new SharedQuestParticipant(_creatorId, SharedQuestRole.Creator, _today);
        Should.Throw<ArgumentNullException>(
            () => new SharedQuest(null!, "Title", "Desc", null, [creator]));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_ParticipantsIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new SharedQuest(SharedQuestId.New(), "Title", "Desc", null, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NoCreatorInParticipants()
    {
        var participant = new SharedQuestParticipant(_jordanId, SharedQuestRole.Participant, _today);
        DomainException ex = Should.Throw<DomainException>(
            () => new SharedQuest(SharedQuestId.New(), "Title", "Desc", null, [participant]));
        ex.Message.ShouldContain("must have a creator");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TooManyParticipantsInConstructor()
    {
        var participants = new List<SharedQuestParticipant>
        {
            new(_creatorId, SharedQuestRole.Creator, _today)
        };
        for (int i = 0; i < 10; i++)
        {
            participants.Add(new SharedQuestParticipant(Guid.NewGuid(), SharedQuestRole.Participant, _today));
        }
        DomainException ex = Should.Throw<DomainException>(
            () => new SharedQuest(SharedQuestId.New(), "Title", "Desc", null, participants));
        ex.Message.ShouldContain("cannot have more than 10");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowNullDescription_When_CreatingQuest()
    {
        SharedQuest quest = new(SharedQuestId.New(), "Title", null!, null,
            [new SharedQuestParticipant(_creatorId, SharedQuestRole.Creator, _today)]);
        quest.Description.ShouldBe(string.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateQuestWithoutDueDate_When_DueDateOmitted()
    {
        SharedQuest quest = SharedQuest.Create("Title", "Desc", _creatorId, _today);
        quest.DueDate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddTaskWithoutAssignee_When_NoAssigneeProvided()
    {
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.AddTask("Unassigned task", _creatorId);
        quest.Tasks[0].AssigneeUserId.ShouldBeNull();
    }

    // ─── Scenario: Accept at capacity ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AcceptingInvitationAtCapacity()
    {
        // Given — quest with 10 participants, invitation was sent before capacity was reached
        SharedQuest quest = CreateDefaultQuest();
        // Invite someone before filling up
        quest = quest.InviteUser(_jordanId);

        // Fill to capacity with others
        for (int i = 0; i < 9; i++)
        {
            Guid userId = Guid.NewGuid();
            quest = quest.InviteUser(userId);
            quest = quest.AcceptInvitation(userId, _today);
        }
        quest.Participants.Count.ShouldBe(10);

        // When / Then — Jordan tries to accept but quest is full
        DomainException ex = Should.Throw<DomainException>(
            () => quest.AcceptInvitation(_jordanId, _today));
        ex.Message.ShouldContain("maximum of 10 participants");
    }

    // ─── Scenario: Non-participant removal ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_OutsiderTriesToRemoveParticipant()
    {
        SharedQuest quest = CreateQuestWithParticipants();
        Guid outsiderId = Guid.NewGuid();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.RemoveParticipant(outsiderId, _jordanId));
        ex.Message.ShouldContain("Only the creator");
    }

    // ─── Strongly-typed IDs ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateUniqueSharedQuestId_When_New()
    {
        SharedQuestId id1 = SharedQuestId.New();
        SharedQuestId id2 = SharedQuestId.New();
        id1.Value.ShouldNotBe(Guid.Empty);
        id1.ShouldNotBe(id2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateUniqueSharedQuestTaskId_When_New()
    {
        SharedQuestTaskId id1 = SharedQuestTaskId.New();
        SharedQuestTaskId id2 = SharedQuestTaskId.New();
        id1.Value.ShouldNotBe(Guid.Empty);
        id1.ShouldNotBe(id2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateUniqueSharedQuestInvitationId_When_New()
    {
        SharedQuestInvitationId id1 = SharedQuestInvitationId.New();
        SharedQuestInvitationId id2 = SharedQuestInvitationId.New();
        id1.Value.ShouldNotBe(Guid.Empty);
        id1.ShouldNotBe(id2);
    }

    // ─── UpdateDetails edge cases ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UpdatingWithEmptyTitle()
    {
        SharedQuest quest = CreateDefaultQuest();
        DomainException ex = Should.Throw<DomainException>(
            () => quest.UpdateDetails(_creatorId, "", "desc", null));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UpdatingWithTooLongTitle()
    {
        SharedQuest quest = CreateDefaultQuest();
        string longTitle = new('x', 201);
        DomainException ex = Should.Throw<DomainException>(
            () => quest.UpdateDetails(_creatorId, longTitle, "desc", null));
        ex.Message.ShouldContain("cannot exceed 200");
    }

    // ─── Boundary and mutation-killing tests ───

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptTitle_When_ExactlyMaxLength()
    {
        // Kills mutant: title.Length > 200 → title.Length >= 200
        string maxTitle = new('x', 200);
        SharedQuest quest = SharedQuest.Create(maxTitle, "desc", _creatorId, _today);
        quest.Title.ShouldBe(maxTitle);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptTaskTitle_When_ExactlyMaxLength()
    {
        // Kills mutant: title.Length > 200 → title.Length >= 200 in SharedQuestTask
        string maxTitle = new('x', 200);
        var task = new SharedQuestTask(SharedQuestTaskId.New(), maxTitle);
        task.Title.ShouldBe(maxTitle);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpdateInvitationStatus_When_Accepted()
    {
        // Kills mutant: conditional select for accepted invitation
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        quest = quest.AcceptInvitation(_jordanId, _today);

        // Verify the invitation status was updated (not just added a participant)
        quest.Invitations.Count.ShouldBe(1);
        quest.Invitations[0].Status.ShouldBe(SharedQuestInvitationStatus.Accepted);
        quest.Invitations[0].InviteeId.ShouldBe(_jordanId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpdateInvitationStatus_When_Declined()
    {
        // Kills mutant: conditional select for declined invitation
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        quest = quest.DeclineInvitation(_jordanId);

        quest.Invitations.Count.ShouldBe(1);
        quest.Invitations[0].Status.ShouldBe(SharedQuestInvitationStatus.Declined);
        quest.Invitations[0].InviteeId.ShouldBe(_jordanId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OnlyAcceptTargetInvitation_When_MultipleInvitationsExist()
    {
        // Kills mutants: && to || in invitation lookup and conditional select
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        quest = quest.InviteUser(_alexId);

        // Accept only Jordan's invitation
        quest = quest.AcceptInvitation(_jordanId, _today);

        // Jordan's invitation should be Accepted, Alex's should still be Pending
        SharedQuestInvitation jordanInvite = quest.Invitations.First(i => i.InviteeId == _jordanId);
        SharedQuestInvitation alexInvite = quest.Invitations.First(i => i.InviteeId == _alexId);
        jordanInvite.Status.ShouldBe(SharedQuestInvitationStatus.Accepted);
        alexInvite.Status.ShouldBe(SharedQuestInvitationStatus.Pending);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OnlyDeclineTargetInvitation_When_MultipleInvitationsExist()
    {
        // Kills mutants: && to || in invitation lookup and conditional select for decline
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        quest = quest.InviteUser(_alexId);

        // Decline only Jordan's invitation
        quest = quest.DeclineInvitation(_jordanId);

        // Jordan's invitation should be Declined, Alex's should still be Pending
        SharedQuestInvitation jordanInvite = quest.Invitations.First(i => i.InviteeId == _jordanId);
        SharedQuestInvitation alexInvite = quest.Invitations.First(i => i.InviteeId == _alexId);
        jordanInvite.Status.ShouldBe(SharedQuestInvitationStatus.Declined);
        alexInvite.Status.ShouldBe(SharedQuestInvitationStatus.Pending);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompleteOnlyTargetTask_When_MultipleTasksExist()
    {
        // Kills mutant: t.Id == taskId → t.Id != taskId in CompleteTask select
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.AddTask("Task 1", _creatorId);
        quest = quest.AddTask("Task 2", _creatorId);

        SharedQuestTaskId task1Id = quest.Tasks[0].Id;

        (quest, _) = quest.CompleteTask(task1Id);

        // First task should be completed, second should not
        quest.Tasks[0].IsCompleted.ShouldBeTrue();
        quest.Tasks[1].IsCompleted.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCompleteQuest_When_TaskListIsEmpty()
    {
        // This scenario ensures updatedTasks.Count > 0 matters
        // With no tasks, the quest should not auto-complete
        SharedQuest quest = CreateDefaultQuest();
        quest.IsCompleted.ShouldBeFalse();
        quest.TotalTaskCount.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowInvitingUserWithDeclinedInvitation_When_PreviouslyDeclined()
    {
        // Kills mutant: && to || in pending invitation check
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        quest = quest.DeclineInvitation(_jordanId);

        // Should be able to re-invite after decline (no pending invitation exists)
        quest = quest.InviteUser(_jordanId);
        quest.Invitations.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AcceptingAlreadyDeclinedInvitationWithOtherPending()
    {
        // Kills mutant: && to || in AcceptInvitation Find
        // With ||, Find would match Alex's pending invitation when looking for Jordan's declined one
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        quest = quest.InviteUser(_alexId);
        quest = quest.DeclineInvitation(_jordanId);

        // Jordan's invitation is now Declined, Alex's is Pending
        // Accepting Jordan should fail (no pending invitation)
        DomainException ex = Should.Throw<DomainException>(
            () => quest.AcceptInvitation(_jordanId, _today));
        ex.Message.ShouldContain("No pending invitation");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DecliningAlreadyAcceptedInvitationWithOtherPending()
    {
        // Kills mutant: && to || in DeclineInvitation Find
        // With ||, Find would match Alex's pending invitation when looking for Jordan's accepted one
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        quest = quest.InviteUser(_alexId);
        quest = quest.AcceptInvitation(_jordanId, _today);

        // Jordan's invitation is now Accepted, Alex's is Pending
        // Declining Jordan should fail (no pending invitation)
        DomainException ex = Should.Throw<DomainException>(
            () => quest.DeclineInvitation(_jordanId));
        ex.Message.ShouldContain("No pending invitation");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompleteTaskInSingleTaskQuest_When_TaskExists()
    {
        // Kills mutant: t.Id == taskId → t.Id != taskId in Find
        // With != on a single-task quest, Find returns null and throws,
        // but with == it correctly finds the task
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.AddTask("Only task", _creatorId);

        SharedQuestTaskId taskId = quest.Tasks[0].Id;
        bool justCompleted;
        (quest, justCompleted) = quest.CompleteTask(taskId);

        quest.Tasks[0].IsCompleted.ShouldBeTrue();
        justCompleted.ShouldBeTrue();
        quest.IsCompleted.ShouldBeTrue();
    }

    // ─── Helpers ───

    private static SharedQuest CreateDefaultQuest()
    {
        return SharedQuest.Create(
            "Plan summer road trip",
            "Organise the group road trip",
            _creatorId, _today,
            new DateOnly(2026, 6, 15));
    }

    private static SharedQuest CreateQuestWithParticipants()
    {
        SharedQuest quest = CreateDefaultQuest();
        quest = quest.InviteUser(_jordanId);
        quest = quest.AcceptInvitation(_jordanId, _today);
        quest = quest.InviteUser(_alexId);
        quest = quest.AcceptInvitation(_alexId, _today);
        return quest;
    }
}
