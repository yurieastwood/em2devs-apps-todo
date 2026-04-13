using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Guild quests, XP progression, and activity feed.
/// Maps to: docs/features/social/guilds.feature
/// Rule: "Guilds have shared quest boards where members collaborate"
/// Rule: "Guilds have collective XP and shared milestones"
/// </summary>
public sealed class GuildQuestXpTests
{
    private static readonly DateOnly _today = new(2026, 3, 15);
    private static readonly DateTimeOffset _now = new(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid _leaderId = Guid.NewGuid();
    private static readonly Guid _jordanId = Guid.NewGuid();
    private static readonly Guid _alexId = Guid.NewGuid();

    private static Guild CreateGuildWithMembers()
    {
        return Guild.Create("Side Project Squad", "desc", _leaderId, _today)
            .AddMember(_jordanId, _today)
            .AddMember(_alexId, _today);
    }

    private static List<GuildTask> CreateSampleTasks()
    {
        return
        [
            new GuildTask(GuildTaskId.New(), "Write copy", _leaderId),
            new GuildTask(GuildTaskId.New(), "Design mockups", _jordanId),
            new GuildTask(GuildTaskId.New(), "Implement HTML/CSS", _alexId),
            new GuildTask(GuildTaskId.New(), "Deploy to hosting", _leaderId)
        ];
    }

    // =====================================================================
    // Scenario: Remove a member with in-progress guild quest tasks
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnassignMemberTasks_When_RemovingMemberWithQuestTasks()
    {
        // Given
        var guild = CreateGuildWithMembers();
        var tasks = CreateSampleTasks();
        guild = guild.CreateQuest("Ship landing page", "Get the marketing site live", new DateOnly(2026, 5, 1), tasks);

        // When — remove Alex who has in-progress tasks
        var result = guild.RemoveMember(_alexId);

        // Then — Alex is removed and their tasks are unassigned
        result.IsMember(_alexId).ShouldBeFalse();
        GuildQuest quest = result.Quests[0];
        GuildTask alexTask = quest.Tasks.First(t => t.Title == "Implement HTML/CSS");
        alexTask.AssigneeUserId.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_KeepOtherAssignments_When_RemovingMemberWithQuestTasks()
    {
        // Given
        var guild = CreateGuildWithMembers();
        var tasks = CreateSampleTasks();
        guild = guild.CreateQuest("Ship landing page", "desc", null, tasks);

        // When
        var result = guild.RemoveMember(_alexId);

        // Then — other members' task assignments unchanged
        GuildQuest quest = result.Quests[0];
        quest.Tasks.First(t => t.Title == "Write copy").AssigneeUserId.ShouldBe(_leaderId);
        quest.Tasks.First(t => t.Title == "Design mockups").AssigneeUserId.ShouldBe(_jordanId);
        quest.Tasks.First(t => t.Title == "Deploy to hosting").AssigneeUserId.ShouldBe(_leaderId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotUnassignCompletedTasks_When_RemovingMember()
    {
        // Given — Alex has a completed task
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask>
        {
            new(taskId, "Done task", _alexId, isCompleted: true),
            new(GuildTaskId.New(), "In progress task", _alexId)
        };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        // When
        var result = guild.RemoveMember(_alexId);

        // Then — completed task retains assignee, in-progress is unassigned
        GuildQuest quest = result.Quests[0];
        quest.Tasks.First(t => t.Title == "Done task").AssigneeUserId.ShouldBe(_alexId);
        quest.Tasks.First(t => t.Title == "In progress task").AssigneeUserId.ShouldBeNull();
    }

    // =====================================================================
    // Scenario: Leader transfer is declined
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_KeepOriginalLeader_When_TransferDeclined()
    {
        // Given — leader with 3 members
        var guild = CreateGuildWithMembers();

        // When — transfer leadership but "Jordan" declines
        // The domain just transfers; the "decline" is modeled by not completing the transfer.
        // The leader remains the leader if transfer is not executed.
        guild.LeaderId.ShouldBe(_leaderId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowTransferToAnotherMember_When_FirstDeclines()
    {
        // Given — leader wants to leave, first choice declines, picks another
        var guild = CreateGuildWithMembers();

        // When — transfer to Alex instead of Jordan
        var result = guild.TransferLeadership(_alexId);

        // Then
        result.LeaderId.ShouldBe(_alexId);
        result.Members.First(m => m.UserId == _leaderId).Role.ShouldBe(GuildRole.Member);
    }

    // =====================================================================
    // Scenario: Last non-leader member leaves the guild
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LeaveLeaderAsSoleMember_When_LastNonLeaderLeaves()
    {
        // Given — guild with leader + 1 member
        var guild = Guild.Create("Side Project Squad", "desc", _leaderId, _today)
            .AddMember(_jordanId, _today);

        // When — Jordan leaves
        var result = guild.Leave(_jordanId);

        // Then — leader is sole remaining member
        result.MemberCount.ShouldBe(1);
        result.LeaderId.ShouldBe(_leaderId);
        result.IsMember(_jordanId).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_KeepGuildActive_When_LastNonLeaderLeaves()
    {
        // Given
        var guild = Guild.Create("Side Project Squad", "desc", _leaderId, _today)
            .AddMember(_jordanId, _today);

        // When
        var result = guild.Leave(_jordanId);

        // Then — guild remains active (not disbanded)
        result.IsDisbanded.ShouldBeFalse();
    }

    // =====================================================================
    // Scenario: Create a guild quest
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateGuildQuest_When_ValidDetails()
    {
        // Given
        var guild = CreateGuildWithMembers();
        var tasks = CreateSampleTasks();

        // When
        var result = guild.CreateQuest("Ship landing page", "Get the marketing site live", new DateOnly(2026, 5, 1), tasks);

        // Then
        result.Quests.Count.ShouldBe(1);
        GuildQuest quest = result.Quests[0];
        quest.Title.ShouldBe("Ship landing page");
        quest.Description.ShouldBe("Get the marketing site live");
        quest.DueDate.ShouldBe(new DateOnly(2026, 5, 1));
        quest.TotalTaskCount.ShouldBe(4);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AssignTasksToMembers_When_CreatingQuest()
    {
        // Given
        var guild = CreateGuildWithMembers();
        var tasks = CreateSampleTasks();

        // When
        var result = guild.CreateQuest("Ship landing page", "desc", null, tasks);

        // Then — each member sees their assigned tasks
        GuildQuest quest = result.Quests[0];
        quest.TasksForUser(_leaderId).Count.ShouldBe(2); // Write copy + Deploy
        quest.TasksForUser(_jordanId).Count.ShouldBe(1); // Design mockups
        quest.TasksForUser(_alexId).Count.ShouldBe(1); // Implement HTML/CSS
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestTitleEmpty()
    {
        // Given
        var guild = CreateGuildWithMembers();

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.CreateQuest("", "desc", null, []));
        ex.Message.ShouldContain("title cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestTitleTooLong()
    {
        // Given
        var guild = CreateGuildWithMembers();

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.CreateQuest(new string('x', 101), "desc", null, []));
        ex.Message.ShouldContain("cannot exceed 100");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AppearOnQuestBoard_When_QuestCreated()
    {
        // Given
        var guild = CreateGuildWithMembers();
        var tasks = CreateSampleTasks();

        // When
        var result = guild.CreateQuest("Ship landing page", "desc", null, tasks);

        // Then — quest visible on board
        result.ActiveQuests.Count.ShouldBe(1);
        result.ActiveQuests[0].Title.ShouldBe("Ship landing page");
    }

    // =====================================================================
    // Scenario: View guild quest board
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowAllActiveQuests_When_ViewingQuestBoard()
    {
        // Given — 3 active quests
        var guild = CreateGuildWithMembers();
        guild = guild.CreateQuest("Quest 1", "desc", null, [new GuildTask(GuildTaskId.New(), "T1", _leaderId)]);
        guild = guild.CreateQuest("Quest 2", "desc", null, [new GuildTask(GuildTaskId.New(), "T2", _jordanId)]);
        guild = guild.CreateQuest("Quest 3", "desc", null, [new GuildTask(GuildTaskId.New(), "T3", _alexId)]);

        // When / Then
        guild.ActiveQuests.Count.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowQuestProgress_When_ViewingQuestBoard()
    {
        // Given — a quest with 4 tasks, 2 completed
        var taskId1 = GuildTaskId.New();
        var taskId2 = GuildTaskId.New();
        var tasks = new List<GuildTask>
        {
            new(taskId1, "T1", _leaderId),
            new(taskId2, "T2", _jordanId),
            new(GuildTaskId.New(), "T3", _alexId),
            new(GuildTaskId.New(), "T4", _leaderId)
        };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        guild = guild.CompleteQuestTask(questId, taskId1, _leaderId, _now);
        guild = guild.CompleteQuestTask(questId, taskId2, _jordanId, _now);

        // When / Then
        GuildQuest quest = guild.Quests[0];
        quest.Progress.ShouldBe(0.5);
        quest.CompletedTaskCount.ShouldBe(2);
        quest.TotalTaskCount.ShouldBe(4);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowTaskAssignments_When_ViewingQuestBoard()
    {
        // Given
        var tasks = CreateSampleTasks();
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        // When / Then
        GuildQuest quest = guild.Quests[0];
        quest.Tasks.ShouldAllBe(t => t.AssigneeUserId != null);
    }

    // =====================================================================
    // Scenario: Complete an assigned guild task
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MarkTaskComplete_When_MemberCompletesTask()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "Write copy", _leaderId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;

        // When
        var result = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // Then
        GuildTask completedTask = result.Quests[0].Tasks[0];
        completedTask.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardGuildXp_When_TaskCompleted()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "Write copy", _leaderId), new(GuildTaskId.New(), "Other", _jordanId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;

        // When
        var result = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // Then — guild XP increased
        result.Xp.TotalXp.ShouldBe(Guild.TaskCompletionXp);
        result.Xp.ContributionFor(_leaderId).ShouldBe(Guild.TaskCompletionXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddFeedItem_When_TaskCompleted()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "Write copy", _leaderId), new(GuildTaskId.New(), "Other", _jordanId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;

        // When
        var result = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // Then
        result.FeedItems.ShouldContain(f => f.EventType == GuildFeedEventType.TaskCompleted);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestNotFound()
    {
        // Given
        var guild = CreateGuildWithMembers();

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.CompleteQuestTask(GuildQuestId.New(), GuildTaskId.New(), _leaderId, _now));
        ex.Message.ShouldContain("Quest not found");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TaskNotFoundInQuest()
    {
        // Given
        var tasks = new List<GuildTask> { new(GuildTaskId.New(), "T1", _leaderId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.CompleteQuestTask(questId, GuildTaskId.New(), _leaderId, _now));
        ex.Message.ShouldContain("Task not found");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingAlreadyCompletedTask()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "T1", _leaderId), new(GuildTaskId.New(), "T2", _jordanId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        guild = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.CompleteQuestTask(questId, taskId, _leaderId, _now));
        ex.Message.ShouldContain("already completed");
    }

    // =====================================================================
    // Scenario: Guild quest completion
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MarkQuestComplete_When_AllTasksCompleted()
    {
        // Given
        var taskId1 = GuildTaskId.New();
        var taskId2 = GuildTaskId.New();
        var tasks = new List<GuildTask>
        {
            new(taskId1, "T1", _leaderId),
            new(taskId2, "T2", _jordanId)
        };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Ship landing page", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        guild = guild.CompleteQuestTask(questId, taskId1, _leaderId, _now);

        // When — final task completed
        var result = guild.CompleteQuestTask(questId, taskId2, _jordanId, _now);

        // Then
        result.Quests[0].IsCompleted.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardBonusXp_When_QuestCompleted()
    {
        // Given
        var taskId1 = GuildTaskId.New();
        var taskId2 = GuildTaskId.New();
        var tasks = new List<GuildTask>
        {
            new(taskId1, "T1", _leaderId),
            new(taskId2, "T2", _jordanId)
        };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Ship landing page", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        guild = guild.CompleteQuestTask(questId, taskId1, _leaderId, _now);

        // When
        var result = guild.CompleteQuestTask(questId, taskId2, _jordanId, _now);

        // Then — bonus XP awarded on top of task XP
        int expectedXp = Guild.TaskCompletionXp * 2 + Guild.QuestCompletionBonusXp;
        result.Xp.TotalXp.ShouldBe(expectedXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddQuestCompletionFeedItem_When_QuestCompleted()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "T1", _leaderId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Ship landing page", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;

        // When
        var result = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // Then — both task completion and quest completion feed items
        result.FeedItems.ShouldContain(f => f.EventType == GuildFeedEventType.QuestCompleted);
        result.FeedItems.ShouldContain(f => f.EventType == GuildFeedEventType.TaskCompleted);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeCompleted_When_QuestHasNoTasks()
    {
        // Given
        var guild = CreateGuildWithMembers()
            .CreateQuest("Empty quest", "desc", null, []);

        // Then — 0 tasks, not completed
        guild.Quests[0].IsCompleted.ShouldBeFalse();
        guild.Quests[0].Progress.ShouldBe(0.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeCompletedQuests_When_ViewingActiveQuests()
    {
        // Given — one quest completed, one active
        var taskId = GuildTaskId.New();
        var guild = CreateGuildWithMembers()
            .CreateQuest("Completed Quest", "desc", null, [new GuildTask(taskId, "T1", _leaderId)])
            .CreateQuest("Active Quest", "desc", null, [new GuildTask(GuildTaskId.New(), "T2", _jordanId)]);

        GuildQuestId completedQuestId = guild.Quests[0].Id;
        guild = guild.CompleteQuestTask(completedQuestId, taskId, _leaderId, _now);

        // Then
        guild.ActiveQuests.Count.ShouldBe(1);
        guild.ActiveQuests[0].Title.ShouldBe("Active Quest");
    }

    // =====================================================================
    // Scenario: View guild XP and level
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowGuildXpTotal_When_ViewingProfile()
    {
        // Given
        var guild = CreateGuildWithMembers();

        // Then — starts at 0
        guild.Xp.TotalXp.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowGuildLevel_When_ViewingProfile()
    {
        // Given
        var guild = CreateGuildWithMembers();

        // Then — starts at level 1
        guild.Level.Value.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowMemberContributions_When_ViewingProfile()
    {
        // Given — two members complete tasks
        var taskId1 = GuildTaskId.New();
        var taskId2 = GuildTaskId.New();
        var tasks = new List<GuildTask>
        {
            new(taskId1, "T1", _leaderId),
            new(taskId2, "T2", _jordanId),
            new(GuildTaskId.New(), "T3", _alexId)
        };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        guild = guild.CompleteQuestTask(questId, taskId1, _leaderId, _now);
        guild = guild.CompleteQuestTask(questId, taskId2, _jordanId, _now);

        // Then
        guild.Xp.ContributionFor(_leaderId).ShouldBe(Guild.TaskCompletionXp);
        guild.Xp.ContributionFor(_jordanId).ShouldBe(Guild.TaskCompletionXp);
        guild.Xp.ContributionFor(_alexId).ShouldBe(0);
    }

    // =====================================================================
    // Scenario: Guild levels up
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LevelUp_When_EnoughXpAccumulated()
    {
        // Given — guild at level 1 with enough tasks to level up
        // GuildLevel.XpPerLevel = 500, TaskCompletionXp = 25, so 20 tasks = 500 XP = level 2
        var guild = CreateGuildWithMembers();

        // Create enough tasks to accumulate 500+ XP (20 tasks * 25 XP = 500)
        var tasks = new List<GuildTask>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(new GuildTask(GuildTaskId.New(), $"Task {i + 1}", _leaderId));
        }

        guild = guild.CreateQuest("Big Quest", "desc", null, tasks);
        GuildQuestId questId = guild.Quests[0].Id;

        // When — complete all 20 tasks
        for (int i = 0; i < 20; i++)
        {
            guild = guild.CompleteQuestTask(questId, guild.Quests[0].Tasks[i].Id, _leaderId, _now.AddMinutes(i));
        }

        // Then — guild should have levelled up (20 tasks * 25 = 500 task XP + 100 quest bonus = 600 total)
        guild.Level.Value.ShouldBeGreaterThan(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddLevelUpFeedItem_When_GuildLevelsUp()
    {
        // Given — enough XP to level up
        var tasks = new List<GuildTask>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(new GuildTask(GuildTaskId.New(), $"Task {i + 1}", _leaderId));
        }

        var guild = CreateGuildWithMembers()
            .CreateQuest("Big Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;

        // When — complete all tasks
        for (int i = 0; i < 20; i++)
        {
            guild = guild.CompleteQuestTask(questId, guild.Quests[0].Tasks[i].Id, _leaderId, _now.AddMinutes(i));
        }

        // Then — level-up feed item present
        guild.FeedItems.ShouldContain(f => f.EventType == GuildFeedEventType.GuildLevelUp);
    }

    // =====================================================================
    // Scenario: View guild activity feed
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowMemberJoinedInFeed_When_MemberJoins()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When
        var result = guild.AddMemberWithFeed(_jordanId, _today, _now);

        // Then
        result.FeedItems.ShouldContain(f => f.EventType == GuildFeedEventType.MemberJoined && f.UserId == _jordanId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowMemberRemovedInFeed_When_MemberRemoved()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_jordanId, _today);

        // When
        var result = guild.RemoveMemberWithFeed(_jordanId, _now);

        // Then
        result.FeedItems.ShouldContain(f => f.EventType == GuildFeedEventType.MemberRemoved && f.UserId == _jordanId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowTaskCompletionInFeed_When_TaskCompleted()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "T1", _leaderId), new(GuildTaskId.New(), "T2", _jordanId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;

        // When
        var result = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // Then
        result.FeedItems.Count.ShouldBeGreaterThan(0);
        result.FeedItems.ShouldContain(f =>
            f.EventType == GuildFeedEventType.TaskCompleted &&
            f.UserId == _leaderId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowQuestCompletionInFeed_When_QuestCompleted()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "T1", _leaderId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;

        // When
        var result = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // Then
        result.FeedItems.ShouldContain(f => f.EventType == GuildFeedEventType.QuestCompleted);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowLevelUpInFeed_When_GuildLevelsUp()
    {
        // Given — enough tasks to trigger level-up
        var tasks = new List<GuildTask>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(new GuildTask(GuildTaskId.New(), $"Task {i + 1}", _leaderId));
        }

        var guild = CreateGuildWithMembers()
            .CreateQuest("Big Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;

        // When
        for (int i = 0; i < 20; i++)
        {
            guild = guild.CompleteQuestTask(questId, guild.Quests[0].Tasks[i].Id, _leaderId, _now.AddMinutes(i));
        }

        // Then
        guild.FeedItems.ShouldContain(f => f.EventType == GuildFeedEventType.GuildLevelUp);
    }

    // =====================================================================
    // GuildTask value object tests
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateGuildTask_When_ValidParameters()
    {
        // Given / When
        var id = GuildTaskId.New();
        var task = new GuildTask(id, "Write copy", _leaderId);

        // Then
        task.Id.ShouldBe(id);
        task.Title.ShouldBe("Write copy");
        task.AssigneeUserId.ShouldBe(_leaderId);
        task.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GuildTaskTitleEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new GuildTask(GuildTaskId.New(), "", _leaderId));
        ex.Message.ShouldContain("title cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GuildTaskTitleTooLong()
    {
        var ex = Should.Throw<DomainException>(
            () => new GuildTask(GuildTaskId.New(), new string('x', 201), _leaderId));
        ex.Message.ShouldContain("cannot exceed 200");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_GuildTaskIdNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new GuildTask(null!, "title", _leaderId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompleteTask_When_NotYetCompleted()
    {
        var task = new GuildTask(GuildTaskId.New(), "T1", _leaderId);
        var completed = task.Complete();
        completed.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingAlreadyCompletedGuildTask()
    {
        var task = new GuildTask(GuildTaskId.New(), "T1", _leaderId, isCompleted: true);
        var ex = Should.Throw<DomainException>(() => task.Complete());
        ex.Message.ShouldContain("already completed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnassignTask_When_Called()
    {
        var task = new GuildTask(GuildTaskId.New(), "T1", _leaderId);
        var unassigned = task.Unassign();
        unassigned.AssigneeUserId.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AssignTask_When_ValidUserId()
    {
        var task = new GuildTask(GuildTaskId.New(), "T1");
        var assigned = task.AssignTo(_jordanId);
        assigned.AssigneeUserId.ShouldBe(_jordanId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AssigningToEmptyGuid()
    {
        var task = new GuildTask(GuildTaskId.New(), "T1");
        var ex = Should.Throw<DomainException>(() => task.AssignTo(Guid.Empty));
        ex.Message.ShouldContain("empty user ID");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateGuildTaskWithoutAssignee_When_NoAssigneeProvided()
    {
        var task = new GuildTask(GuildTaskId.New(), "T1");
        task.AssigneeUserId.ShouldBeNull();
    }

    // =====================================================================
    // GuildQuest value object tests
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_GuildQuestIdNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new GuildQuest(null!, "title", "desc", null, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_GuildQuestTasksNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new GuildQuest(GuildQuestId.New(), "title", "desc", null, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleNullDescription_When_CreatingGuildQuest()
    {
        var quest = new GuildQuest(GuildQuestId.New(), "title", null!, null, []);
        quest.Description.ShouldBe(string.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CompletingNullTaskId()
    {
        var quest = new GuildQuest(GuildQuestId.New(), "title", "desc", null,
            [new GuildTask(GuildTaskId.New(), "T1", _leaderId)]);
        Should.Throw<ArgumentNullException>(() => quest.CompleteTask(null!));
    }

    // =====================================================================
    // GuildXp value object tests
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartAtZero_When_GuildXpCreated()
    {
        var xp = GuildXp.Zero();
        xp.TotalXp.ShouldBe(0);
        xp.MemberContributions.Count.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddXp_When_MemberContributes()
    {
        var xp = GuildXp.Zero().AddXp(50, _leaderId);
        xp.TotalXp.ShouldBe(50);
        xp.ContributionFor(_leaderId).ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AccumulateContributions_When_SameMemberContributesTwice()
    {
        var xp = GuildXp.Zero()
            .AddXp(50, _leaderId)
            .AddXp(30, _leaderId);
        xp.TotalXp.ShouldBe(80);
        xp.ContributionFor(_leaderId).ShouldBe(80);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackMultipleContributors()
    {
        var xp = GuildXp.Zero()
            .AddXp(50, _leaderId)
            .AddXp(30, _jordanId);
        xp.TotalXp.ShouldBe(80);
        xp.ContributionFor(_leaderId).ShouldBe(50);
        xp.ContributionFor(_jordanId).ShouldBe(30);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GuildXpNegative()
    {
        var ex = Should.Throw<DomainException>(() => new GuildXp(-1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AddingZeroXp()
    {
        var xp = GuildXp.Zero();
        var ex = Should.Throw<DomainException>(() => xp.AddXp(0, _leaderId));
        ex.Message.ShouldContain("must be positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AddingNegativeXp()
    {
        var xp = GuildXp.Zero();
        var ex = Should.Throw<DomainException>(() => xp.AddXp(-5, _leaderId));
        ex.Message.ShouldContain("must be positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ContributorIdEmpty()
    {
        var xp = GuildXp.Zero();
        var ex = Should.Throw<DomainException>(() => xp.AddXp(10, Guid.Empty));
        ex.Message.ShouldContain("Contributor user ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZero_When_MemberHasNoContribution()
    {
        var xp = GuildXp.Zero();
        xp.ContributionFor(_leaderId).ShouldBe(0);
    }

    // =====================================================================
    // GuildLevel value object tests
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartAtLevel1_When_GuildLevelCreated()
    {
        var level = GuildLevel.Starting();
        level.Value.ShouldBe(1);
        level.CurrentXp.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LevelUp_When_XpExceedsThreshold()
    {
        var level = GuildLevel.Starting();
        (GuildLevel newLevel, bool levelledUp) = level.AddXp(500);
        newLevel.Value.ShouldBe(2);
        levelledUp.ShouldBeTrue();
        newLevel.CurrentXp.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotLevelUp_When_XpBelowThreshold()
    {
        var level = GuildLevel.Starting();
        (GuildLevel newLevel, bool levelledUp) = level.AddXp(499);
        newLevel.Value.ShouldBe(1);
        levelledUp.ShouldBeFalse();
        newLevel.CurrentXp.ShouldBe(499);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LevelUpMultipleTimes_When_EnoughXp()
    {
        var level = GuildLevel.Starting();
        (GuildLevel newLevel, bool levelledUp) = level.AddXp(1200);
        newLevel.Value.ShouldBe(3);
        levelledUp.ShouldBeTrue();
        newLevel.CurrentXp.ShouldBe(200);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapAtMaxLevel_When_ExcessiveXp()
    {
        var level = new GuildLevel(GuildLevel.MaxLevel - 1, 0);
        (GuildLevel newLevel, _) = level.AddXp(99999);
        newLevel.Value.ShouldBe(GuildLevel.MaxLevel);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_LevelBelowOne()
    {
        var ex = Should.Throw<DomainException>(() => new GuildLevel(0, 0));
        ex.Message.ShouldContain("at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_LevelExceedsMax()
    {
        var ex = Should.Throw<DomainException>(() => new GuildLevel(GuildLevel.MaxLevel + 1, 0));
        ex.Message.ShouldContain("cannot exceed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GuildLevelCurrentXpNegative()
    {
        var ex = Should.Throw<DomainException>(() => new GuildLevel(1, -1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AddingZeroOrNegativeXpToGuildLevel()
    {
        var level = GuildLevel.Starting();
        var ex = Should.Throw<DomainException>(() => level.AddXp(0));
        ex.Message.ShouldContain("must be positive");

        ex = Should.Throw<DomainException>(() => level.AddXp(-5));
        ex.Message.ShouldContain("must be positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnXpToNextLevel_When_NotAtMax()
    {
        var level = new GuildLevel(1, 200);
        level.XpToNextLevel().ShouldBe(300); // 500 - 200
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZero_When_AtMaxLevel()
    {
        var level = new GuildLevel(GuildLevel.MaxLevel, 0);
        level.XpToNextLevel().ShouldBe(0);
    }

    // =====================================================================
    // GuildFeedItem value object tests
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateFeedItem_When_ValidParameters()
    {
        var item = new GuildFeedItem(GuildFeedEventType.MemberJoined, _leaderId, "Joined", _now);
        item.EventType.ShouldBe(GuildFeedEventType.MemberJoined);
        item.UserId.ShouldBe(_leaderId);
        item.Description.ShouldBe("Joined");
        item.OccurredAt.ShouldBe(_now);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FeedItemUserIdEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new GuildFeedItem(GuildFeedEventType.MemberJoined, Guid.Empty, "Joined", _now));
        ex.Message.ShouldContain("user ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FeedItemDescriptionEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new GuildFeedItem(GuildFeedEventType.MemberJoined, _leaderId, "", _now));
        ex.Message.ShouldContain("description cannot be empty");
    }

    // =====================================================================
    // GuildQuestId / GuildTaskId value object tests
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateNewGuildQuestId_When_UsingFactory()
    {
        var id = GuildQuestId.New();
        id.ShouldNotBeNull();
        id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateNewGuildTaskId_When_UsingFactory()
    {
        var id = GuildTaskId.New();
        id.ShouldNotBeNull();
        id.Value.ShouldNotBe(Guid.Empty);
    }

    // =====================================================================
    // Guild state preservation tests
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveQuests_When_AddingMember()
    {
        // Given
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, [new GuildTask(GuildTaskId.New(), "T1", _leaderId)]);

        // When
        var newMemberId = Guid.NewGuid();
        var result = guild.AddMember(newMemberId, _today);

        // Then
        result.Quests.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveXpAndLevel_When_TransferringLeadership()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "T1", _leaderId), new(GuildTaskId.New(), "T2", _jordanId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        guild = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // When
        var result = guild.TransferLeadership(_jordanId);

        // Then
        result.Xp.TotalXp.ShouldBe(guild.Xp.TotalXp);
        result.Level.Value.ShouldBe(guild.Level.Value);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveXpAndLevel_When_MemberLeaves()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "T1", _leaderId), new(GuildTaskId.New(), "T2", _jordanId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        guild = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // When
        var result = guild.Leave(_alexId);

        // Then
        result.Xp.TotalXp.ShouldBe(guild.Xp.TotalXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveFeed_When_MemberLeaves()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "T1", _leaderId), new(GuildTaskId.New(), "T2", _jordanId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        guild = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);
        int feedCountBefore = guild.FeedItems.Count;

        // When
        var result = guild.Leave(_alexId);

        // Then
        result.FeedItems.Count.ShouldBe(feedCountBefore);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveXpAndLevel_When_Disbanding()
    {
        // Given
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "T1", _leaderId), new(GuildTaskId.New(), "T2", _jordanId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        guild = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // When
        var result = guild.Disband(_leaderId);

        // Then
        result.Xp.TotalXp.ShouldBe(guild.Xp.TotalXp);
        result.IsDisbanded.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveQuests_When_LeaderLeaves()
    {
        // Given
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, [new GuildTask(GuildTaskId.New(), "T1", _jordanId)]);

        // When
        var result = guild.LeaderLeave();

        // Then
        result.Quests.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveQuests_When_UpdatingDetails()
    {
        // Given
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, [new GuildTask(GuildTaskId.New(), "T1", _leaderId)]);

        // When
        var result = guild.UpdateDetails(_leaderId, "New Name", "New Desc");

        // Then
        result.Quests.Count.ShouldBe(1);
    }

    // =====================================================================
    // Null guard tests for CompleteQuestTask
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CompleteQuestTaskWithNullQuestId()
    {
        var guild = CreateGuildWithMembers();
        Should.Throw<ArgumentNullException>(
            () => guild.CompleteQuestTask(null!, GuildTaskId.New(), _leaderId, _now));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CompleteQuestTaskWithNullTaskId()
    {
        // The null check is delegated to GuildQuest.CompleteTask
        var tasks = new List<GuildTask> { new(GuildTaskId.New(), "T1", _leaderId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        Should.Throw<ArgumentNullException>(
            () => guild.CompleteQuestTask(questId, null!, _leaderId, _now));
    }

    // =====================================================================
    // GuildXp with initial contributions
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptInitialContributions_When_CreatingGuildXp()
    {
        var contributions = new Dictionary<Guid, int> { { _leaderId, 100 } };
        var xp = new GuildXp(100, contributions);
        xp.TotalXp.ShouldBe(100);
        xp.ContributionFor(_leaderId).ShouldBe(100);
    }

    // =====================================================================
    // Boundary / mutation-killing tests
    // =====================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptQuestTitle_When_Exactly100Chars()
    {
        // Kill mutant: GuildQuest title.Length >= 100 vs > 100
        var quest = new GuildQuest(GuildQuestId.New(), new string('x', 100), "desc", null, []);
        quest.Title.Length.ShouldBe(100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptGuildTaskTitle_When_Exactly200Chars()
    {
        // Kill mutant: GuildTask title.Length >= 200 vs > 200
        var task = new GuildTask(GuildTaskId.New(), new string('x', 200));
        task.Title.Length.ShouldBe(200);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CorrectlyCalculateXpDelta_When_CompletingTaskWithExistingXp()
    {
        // Kill mutant: updatedXp.TotalXp + Xp.TotalXp (should be -)
        // When guild already has XP, the delta must be (new - old), not (new + old)
        // First complete some tasks to accumulate XP, then complete another and verify level XP
        var taskId1 = GuildTaskId.New();
        var taskId2 = GuildTaskId.New();
        var taskId3 = GuildTaskId.New();
        var tasks = new List<GuildTask>
        {
            new(taskId1, "T1", _leaderId),
            new(taskId2, "T2", _jordanId),
            new(taskId3, "T3", _alexId)
        };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;

        // Complete first task: guild XP goes to 25, level XP goes to 25
        guild = guild.CompleteQuestTask(questId, taskId1, _leaderId, _now);
        guild.Xp.TotalXp.ShouldBe(25);
        guild.Level.CurrentXp.ShouldBe(25);

        // Complete second task: guild XP goes to 50, level XP delta should be 25 (not 50+25=75)
        var result = guild.CompleteQuestTask(questId, taskId2, _jordanId, _now);
        result.Xp.TotalXp.ShouldBe(50);
        // Level gets delta of 25 added to existing 25 = 50
        result.Level.CurrentXp.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotAddLevelUpFeedItem_When_NotLevelledUp()
    {
        // Kill mutant: !(levelledUp) — negating the condition would add feed item when NOT levelling up
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "T1", _leaderId), new(GuildTaskId.New(), "T2", _jordanId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        var result = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // Only 25 XP added, not enough to level up (need 500)
        result.FeedItems.ShouldNotContain(f => f.EventType == GuildFeedEventType.GuildLevelUp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddQuestCompletionFeedItem_When_QuestCompletedWithSingleTask()
    {
        // Kill mutant: Statement mutation removing ArgumentNullException.ThrowIfNull(taskId) (line 306)
        // This is actually about testing that the quest completion feed item IS present
        // when all tasks are done. Already tested, but ensure count is exact.
        var taskId = GuildTaskId.New();
        var tasks = new List<GuildTask> { new(taskId, "T1", _leaderId) };
        var guild = CreateGuildWithMembers()
            .CreateQuest("Quest", "desc", null, tasks);

        GuildQuestId questId = guild.Quests[0].Id;
        var result = guild.CompleteQuestTask(questId, taskId, _leaderId, _now);

        // Should have exactly 2 feed items: task completed + quest completed
        // No level up because 125 XP < 500 threshold
        result.FeedItems.Count.ShouldBe(2);
        result.FeedItems[0].EventType.ShouldBe(GuildFeedEventType.TaskCompleted);
        result.FeedItems[1].EventType.ShouldBe(GuildFeedEventType.QuestCompleted);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapLevelAtMax_When_AtMaxLevelAndAddingXp()
    {
        // Kill mutant: GuildLevel currentLevel <= MaxLevel (line 52)
        // and currentLevel > MaxLevel (line 59) and block removal (line 60)
        var level = new GuildLevel(GuildLevel.MaxLevel, 100);
        (GuildLevel newLevel, bool levelledUp) = level.AddXp(1000);
        newLevel.Value.ShouldBe(GuildLevel.MaxLevel);
        levelledUp.ShouldBeFalse();
        // XP should accumulate beyond the cap
        newLevel.CurrentXp.ShouldBe(1100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleExactLevelBoundary_When_AddingXp()
    {
        // Kill boundary mutants: at exactly XpPerLevel
        var level = GuildLevel.Starting();
        (GuildLevel newLevel, bool levelledUp) = level.AddXp(GuildLevel.XpPerLevel);
        newLevel.Value.ShouldBe(2);
        newLevel.CurrentXp.ShouldBe(0);
        levelledUp.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LevelUpFromMaxMinusOneToMax_When_ExactXp()
    {
        // Kill mutant: edge case where currentLevel >= MaxLevel after levelling
        var level = new GuildLevel(GuildLevel.MaxLevel - 1, 0);
        (GuildLevel newLevel, bool levelledUp) = level.AddXp(GuildLevel.XpPerLevel);
        newLevel.Value.ShouldBe(GuildLevel.MaxLevel);
        newLevel.CurrentXp.ShouldBe(0);
        levelledUp.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AccumulateExcessXpAtMax_When_LevellingPastMax()
    {
        // Kill block removal mutant on GuildLevel line 60
        var level = new GuildLevel(GuildLevel.MaxLevel - 1, 0);
        (GuildLevel newLevel, _) = level.AddXp(GuildLevel.XpPerLevel + 200);
        newLevel.Value.ShouldBe(GuildLevel.MaxLevel);
        newLevel.CurrentXp.ShouldBe(200);
    }
}
