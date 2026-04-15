using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for EnergyTaskSorter domain service.
/// Covers energy-aware task surfacing from energy-scheduling.feature.
/// </summary>
public sealed class EnergyTaskSorterTests
{
    // =================================================================
    // Scenario: Energy level affects task surfacing order
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SurfaceHardTasksFirst_When_EnergyIsPeak()
    {
        // Given
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Refactor auth module"), TaskDifficulty.Hard);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Code review"), TaskDifficulty.Normal);
        var epicTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("System redesign"), TaskDifficulty.Epic);
        var tasks = new List<TodoTask> { easyTask, hardTask, normalTask, epicTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Peak);

        // Then — hard/epic tasks should come before normal, which comes before easy
        sorted[0].Difficulty.ShouldBeOneOf(TaskDifficulty.Epic, TaskDifficulty.Hard);
        sorted[1].Difficulty.ShouldBeOneOf(TaskDifficulty.Epic, TaskDifficulty.Hard);
        sorted[2].Difficulty.ShouldBe(TaskDifficulty.Normal);
        sorted[3].Difficulty.ShouldBe(TaskDifficulty.Easy);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SurfaceHardTasksFirst_When_EnergyIsHigh()
    {
        // Given
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("File expense report"), TaskDifficulty.Easy);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design API"), TaskDifficulty.Hard);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Stand-up meeting"), TaskDifficulty.Normal);
        var tasks = new List<TodoTask> { easyTask, hardTask, normalTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.High);

        // Then — hard tasks first, then normal, then easy
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Hard);
        sorted[1].Difficulty.ShouldBe(TaskDifficulty.Normal);
        sorted[2].Difficulty.ShouldBe(TaskDifficulty.Easy);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SurfaceNormalTasksFirst_When_EnergyIsMedium()
    {
        // Given
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Clean desk"), TaskDifficulty.Easy);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Refactor module"), TaskDifficulty.Hard);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Update docs"), TaskDifficulty.Normal);
        var tasks = new List<TodoTask> { easyTask, hardTask, normalTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Medium);

        // Then — normal first, then easy/trivial or hard (neighbours), then the furthest
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Normal);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SurfaceEasyTasksFirst_When_EnergyIsLow()
    {
        // Given
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design new feature"), TaskDifficulty.Hard);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Code review"), TaskDifficulty.Normal);
        var trivialTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Check notifications"), TaskDifficulty.Trivial);
        var tasks = new List<TodoTask> { hardTask, normalTask, easyTask, trivialTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Low);

        // Then — easy/trivial tasks first, hard tasks last
        sorted[0].Difficulty.ShouldBeOneOf(TaskDifficulty.Trivial, TaskDifficulty.Easy);
        sorted[1].Difficulty.ShouldBeOneOf(TaskDifficulty.Trivial, TaskDifficulty.Easy);
        sorted[2].Difficulty.ShouldBe(TaskDifficulty.Normal);
        sorted[3].Difficulty.ShouldBe(TaskDifficulty.Hard);
    }

    // =================================================================
    // Scenario: Energy-aware reordering does not hide tasks
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnAllTasks_When_SortedByEnergyMatch()
    {
        // Given
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design new feature"), TaskDifficulty.Hard);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Code review"), TaskDifficulty.Normal);
        var tasks = new List<TodoTask> { easyTask, hardTask, normalTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Low);

        // Then — all tasks should be present, none removed
        sorted.Count.ShouldBe(tasks.Count);
        sorted.ShouldContain(easyTask);
        sorted.ShouldContain(hardTask);
        sorted.ShouldContain(normalTask);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnAllTasks_When_SortedByHighEnergy()
    {
        // Given
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design new feature"), TaskDifficulty.Hard);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Code review"), TaskDifficulty.Normal);
        var epicTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("System redesign"), TaskDifficulty.Epic);
        var trivialTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Check inbox"), TaskDifficulty.Trivial);
        var tasks = new List<TodoTask> { easyTask, hardTask, normalTask, epicTask, trivialTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.High);

        // Then — all 5 tasks should still be present
        sorted.Count.ShouldBe(5);
        sorted.ShouldContain(easyTask);
        sorted.ShouldContain(hardTask);
        sorted.ShouldContain(normalTask);
        sorted.ShouldContain(epicTask);
        sorted.ShouldContain(trivialTask);
    }

    // =================================================================
    // Edge cases
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnEmptyList_When_NoTasksProvided()
    {
        // Given
        var tasks = new List<TodoTask>();

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.High);

        // Then
        sorted.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSingleTask_When_OnlyOneTaskProvided()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Single task"), TaskDifficulty.Normal);
        var tasks = new List<TodoTask> { task };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Medium);

        // Then
        sorted.Count.ShouldBe(1);
        sorted[0].ShouldBe(task);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TasksIsNull()
    {
        // Given / When / Then
        var exception = Should.Throw<ArgumentNullException>(
            () => EnergyTaskSorter.SortByEnergyMatch(null!, EnergyLevel.High));
        exception.ParamName.ShouldBe("tasks");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SurfaceEpicAndHardFirst_When_EnergyIsPeakWithMixedDifficulties()
    {
        // Given — verifies that Peak energy surfaces the most difficult tasks
        var trivialTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Check inbox"), TaskDifficulty.Trivial);
        var epicTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("System redesign"), TaskDifficulty.Epic);
        var tasks = new List<TodoTask> { trivialTask, epicTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Peak);

        // Then
        sorted[0].ShouldBe(epicTask);
        sorted[1].ShouldBe(trivialTask);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveAllDifficulties_When_SortedByMediumEnergy()
    {
        // Given
        var trivialTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Check inbox"), TaskDifficulty.Trivial);
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Code review"), TaskDifficulty.Normal);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design API"), TaskDifficulty.Hard);
        var epicTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("System redesign"), TaskDifficulty.Epic);
        var tasks = new List<TodoTask> { trivialTask, easyTask, normalTask, hardTask, epicTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Medium);

        // Then — all tasks preserved, normal first (closest match to Medium)
        sorted.Count.ShouldBe(5);
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Normal);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OrderTrivialBeforeEasy_When_EnergyIsLow()
    {
        // Given — low energy should prefer easiest tasks first
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var trivialTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Check inbox"), TaskDifficulty.Trivial);
        var tasks = new List<TodoTask> { easyTask, trivialTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Low);

        // Then
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Trivial);
        sorted[1].Difficulty.ShouldBe(TaskDifficulty.Easy);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OrderEpicBeforeHard_When_EnergyIsPeak()
    {
        // Given — peak energy should prefer most difficult tasks first
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design API"), TaskDifficulty.Hard);
        var epicTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("System redesign"), TaskDifficulty.Epic);
        var tasks = new List<TodoTask> { hardTask, epicTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Peak);

        // Then
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Epic);
        sorted[1].Difficulty.ShouldBe(TaskDifficulty.Hard);
    }

    // =================================================================
    // Boundary: Tie-breaking tests to kill mutation survivors
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreferHarderTask_When_HighEnergyAndEquidistant()
    {
        // Given — at High energy (target=Hard), Epic and Normal are both distance 1
        // Tie-break: High energy prefers harder, so Epic should come before Normal
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Code review"), TaskDifficulty.Normal);
        var epicTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("System redesign"), TaskDifficulty.Epic);
        var tasks = new List<TodoTask> { normalTask, epicTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.High);

        // Then
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Epic);
        sorted[1].Difficulty.ShouldBe(TaskDifficulty.Normal);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreferEasierTask_When_MediumEnergyAndEquidistant()
    {
        // Given — at Medium energy (target=Normal), Easy and Hard are both distance 1
        // Tie-break: Medium energy prefers easier, so Easy should come before Hard
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design API"), TaskDifficulty.Hard);
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var tasks = new List<TodoTask> { hardTask, easyTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Medium);

        // Then
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Easy);
        sorted[1].Difficulty.ShouldBe(TaskDifficulty.Hard);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreferEasierTask_When_LowEnergyAndEquidistant()
    {
        // Given — at Low energy (target=Trivial), Easy and special equidistant case
        // Verifies that low energy direction tie-breaks toward easier
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Code review"), TaskDifficulty.Normal);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design API"), TaskDifficulty.Hard);
        var tasks = new List<TodoTask> { hardTask, normalTask, easyTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Low);

        // Then — easyTask closest (distance 1), normal (distance 2), hard (distance 3)
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Easy);
        sorted[1].Difficulty.ShouldBe(TaskDifficulty.Normal);
        sorted[2].Difficulty.ShouldBe(TaskDifficulty.Hard);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OrderFullSpectrum_When_EnergyIsHighWithAllDifficulties()
    {
        // Given — comprehensive test with all difficulty levels at High energy
        var trivialTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Check inbox"), TaskDifficulty.Trivial);
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Code review"), TaskDifficulty.Normal);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design API"), TaskDifficulty.Hard);
        var epicTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("System redesign"), TaskDifficulty.Epic);
        var tasks = new List<TodoTask> { trivialTask, easyTask, normalTask, hardTask, epicTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.High);

        // Then — Hard first (distance 0), then Epic (distance 1) before Normal (distance 1, tie-break prefers harder)
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Hard);
        sorted[1].Difficulty.ShouldBe(TaskDifficulty.Epic);
        sorted[2].Difficulty.ShouldBe(TaskDifficulty.Normal);
        sorted[3].Difficulty.ShouldBe(TaskDifficulty.Easy);
        sorted[4].Difficulty.ShouldBe(TaskDifficulty.Trivial);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OrderFullSpectrum_When_EnergyIsLowWithAllDifficulties()
    {
        // Given — comprehensive test with all difficulty levels at Low energy
        var trivialTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Check inbox"), TaskDifficulty.Trivial);
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Code review"), TaskDifficulty.Normal);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design API"), TaskDifficulty.Hard);
        var epicTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("System redesign"), TaskDifficulty.Epic);
        var tasks = new List<TodoTask> { epicTask, hardTask, normalTask, easyTask, trivialTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Low);

        // Then — Trivial first (distance 0), Easy (distance 1), Normal (distance 2), Hard (distance 3), Epic (distance 4)
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Trivial);
        sorted[1].Difficulty.ShouldBe(TaskDifficulty.Easy);
        sorted[2].Difficulty.ShouldBe(TaskDifficulty.Normal);
        sorted[3].Difficulty.ShouldBe(TaskDifficulty.Hard);
        sorted[4].Difficulty.ShouldBe(TaskDifficulty.Epic);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OrderFullSpectrum_When_EnergyIsPeakWithAllDifficulties()
    {
        // Given
        var trivialTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Check inbox"), TaskDifficulty.Trivial);
        var easyTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Reply to email"), TaskDifficulty.Easy);
        var normalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Code review"), TaskDifficulty.Normal);
        var hardTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Design API"), TaskDifficulty.Hard);
        var epicTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("System redesign"), TaskDifficulty.Epic);
        var tasks = new List<TodoTask> { trivialTask, easyTask, normalTask, hardTask, epicTask };

        // When
        var sorted = EnergyTaskSorter.SortByEnergyMatch(tasks, EnergyLevel.Peak);

        // Then — Epic first (distance 0), Hard (distance 1), Normal (distance 2), Easy (distance 3), Trivial (distance 4)
        sorted[0].Difficulty.ShouldBe(TaskDifficulty.Epic);
        sorted[1].Difficulty.ShouldBe(TaskDifficulty.Hard);
        sorted[2].Difficulty.ShouldBe(TaskDifficulty.Normal);
        sorted[3].Difficulty.ShouldBe(TaskDifficulty.Easy);
        sorted[4].Difficulty.ShouldBe(TaskDifficulty.Trivial);
    }
}
