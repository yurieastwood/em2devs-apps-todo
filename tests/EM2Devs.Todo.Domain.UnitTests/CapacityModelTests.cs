using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for CapacityModel entity.
/// Tests encode behaviors from capacity-modelling.feature.
/// </summary>
public sealed class CapacityModelTests
{
    // ==========================================================================
    // Scenario 1: Capacity model established from task completion history
    // ==========================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BuildCapacityModel_When_HistoricalDataProvided()
    {
        // Given I have completed tasks for 30 days with average 6 on weekdays, 3 on weekends
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 6 },
            { DayOfWeek.Tuesday, 6 },
            { DayOfWeek.Wednesday, 6 },
            { DayOfWeek.Thursday, 6 },
            { DayOfWeek.Friday, 6 },
            { DayOfWeek.Saturday, 3 },
            { DayOfWeek.Sunday, 3 },
        };

        // When
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // Then
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(6);
        model.GetCapacity(DayOfWeek.Tuesday).ShouldBe(6);
        model.GetCapacity(DayOfWeek.Wednesday).ShouldBe(6);
        model.GetCapacity(DayOfWeek.Thursday).ShouldBe(6);
        model.GetCapacity(DayOfWeek.Friday).ShouldBe(6);
        model.GetCapacity(DayOfWeek.Saturday).ShouldBe(3);
        model.GetCapacity(DayOfWeek.Sunday).ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroCapacity_When_DayNotInHistory()
    {
        // Given a model with no data for a specific day
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 6 },
        };

        // When
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // Then
        model.GetCapacity(DayOfWeek.Sunday).ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_HistoryContainsNegativeCapacity()
    {
        // Given
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, -1 },
        };

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => CapacityModel.BuildFromHistory(history));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_HistoryIsNull()
    {
        // When / Then
        Should.Throw<ArgumentNullException>(() => CapacityModel.BuildFromHistory(null!));
    }

    // ==========================================================================
    // Scenario 2: Capacity accounts for task difficulty weighting
    // ==========================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectOvercommitment_When_WeightedTasksExceedCapacity()
    {
        // Given my capacity model shows ~18 units (6 Normal tasks * 3 units each)
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Wednesday, 18 }, // 6 Normal tasks * 3 units = 18
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When I have 4 Hard (4*5=20) + 2 Normal (2*3=6) = 26 units
        var tasks = new List<TaskDifficulty?>
        {
            TaskDifficulty.Hard, TaskDifficulty.Hard, TaskDifficulty.Hard, TaskDifficulty.Hard,
            TaskDifficulty.Normal, TaskDifficulty.Normal,
        };
        int scheduledUnits = CapacityModel.CalculateScheduledUnits(tasks);

        // Then
        scheduledUnits.ShouldBe(26);
        model.IsOvercommitted(DayOfWeek.Wednesday, scheduledUnits).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MapCorrectWeights_When_AllDifficultiesUsed()
    {
        // Then each difficulty maps to its expected weight
        DifficultyWeight.For(TaskDifficulty.Trivial).ShouldBe(1);
        DifficultyWeight.For(TaskDifficulty.Easy).ShouldBe(2);
        DifficultyWeight.For(TaskDifficulty.Normal).ShouldBe(3);
        DifficultyWeight.For(TaskDifficulty.Hard).ShouldBe(5);
        DifficultyWeight.For(TaskDifficulty.Epic).ShouldBe(8);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToNormalWeight_When_UnknownDifficultyProvided()
    {
        // Given an invalid enum value
        var unknownDifficulty = (TaskDifficulty)999;

        // When / Then
        DifficultyWeight.For(unknownDifficulty).ShouldBe(DifficultyWeight.Normal);
    }

    // ==========================================================================
    // Scenario 3: Tasks with no difficulty default to Normal weighting
    // ==========================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToNormalWeight_When_DifficultyIsNull()
    {
        // Given tasks with null difficulty
        var tasks = new List<TaskDifficulty?> { null, null };

        // When
        int units = CapacityModel.CalculateScheduledUnits(tasks);

        // Then (2 * Normal weight of 3 = 6)
        units.ShouldBe(6);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MixNullAndExplicitDifficulties_When_Calculating()
    {
        // Given a mix of null and explicit difficulties
        var tasks = new List<TaskDifficulty?> { null, TaskDifficulty.Hard, TaskDifficulty.Trivial };

        // When
        int units = CapacityModel.CalculateScheduledUnits(tasks);

        // Then (Normal=3 + Hard=5 + Trivial=1 = 9)
        units.ShouldBe(9);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TaskDifficultiesCollectionIsNull()
    {
        // When / Then
        Should.Throw<ArgumentNullException>(() => CapacityModel.CalculateScheduledUnits(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroUnits_When_NoTasksScheduled()
    {
        // Given
        var tasks = new List<TaskDifficulty?>();

        // When
        int units = CapacityModel.CalculateScheduledUnits(tasks);

        // Then
        units.ShouldBe(0);
    }

    // ==========================================================================
    // Scenario 4: Capacity model updates gradually with new data
    // ==========================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AdjustCapacityGradually_When_RecalibratedWithHigherData()
    {
        // Given historical weekday capacity is 6 units
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 6 },
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When recalibrated with observed capacity of 8, max adjustment of 1
        // weighted avg = (6*1 + 8*1) / (1+1) = 7, delta = 1, within maxAdjustment
        model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 8, historicalWeight: 1, recentWeight: 1, maxAdjustment: 1);

        // Then capacity should adjust upward but not exceed maxAdjustment of 1
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(7); // 6 + 1
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AdjustCapacityDownwardGradually_When_RecalibratedWithLowerData()
    {
        // Given
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 8 },
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When recalibrated with much lower observed capacity
        model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 2, historicalWeight: 3, recentWeight: 1, maxAdjustment: 1);

        // Then capacity should adjust downward by at most maxAdjustment
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(7); // 8 - capped(1)
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotExceedMaxAdjustment_When_LargeDifferenceBetweenCurrentAndObserved()
    {
        // Given
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 6 },
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When recalibrated with very high observed capacity
        model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 100, historicalWeight: 1, recentWeight: 1, maxAdjustment: 1);

        // Then should only adjust by maxAdjustment (1)
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(7);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AdjustWithinMaxAdjustment_When_WeightedAverageDeltaIsSmall()
    {
        // Given
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 6 },
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When recalibrated with only slightly higher observed capacity and heavy historical weight
        // weighted avg = (6*7 + 7*1) / (7+1) = 49/8 = 6 (integer division), delta = 0
        model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 7, historicalWeight: 7, recentWeight: 1, maxAdjustment: 1);

        // Then capacity stays the same because weighted average rounds to same value
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(6);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ObservedCapacityIsNegative()
    {
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 6 } });

        DomainException ex = Should.Throw<DomainException>(() =>
            model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: -1, historicalWeight: 3, recentWeight: 1, maxAdjustment: 1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_HistoricalWeightIsZeroOrNegative()
    {
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 6 } });

        DomainException ex = Should.Throw<DomainException>(() =>
            model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 8, historicalWeight: 0, recentWeight: 1, maxAdjustment: 1));
        ex.Message.ShouldContain("Historical weight must be positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecentWeightIsZeroOrNegative()
    {
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 6 } });

        DomainException ex = Should.Throw<DomainException>(() =>
            model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 8, historicalWeight: 3, recentWeight: 0, maxAdjustment: 1));
        ex.Message.ShouldContain("Recent weight must be positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MaxAdjustmentIsZeroOrNegative()
    {
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 6 } });

        DomainException ex = Should.Throw<DomainException>(() =>
            model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 8, historicalWeight: 3, recentWeight: 1, maxAdjustment: 0));
        ex.Message.ShouldContain("Max adjustment must be positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecalibrateNewDay_When_DayNotPreviouslyTracked()
    {
        // Given a model with no Tuesday data
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 6 } });

        // When recalibrating Tuesday (current=0)
        model.Recalibrate(DayOfWeek.Tuesday, newObservedCapacity: 8, historicalWeight: 3, recentWeight: 1, maxAdjustment: 1);

        // Then weighted avg = (0*3 + 8*1)/(3+1) = 2, delta = 2, capped to 1
        model.GetCapacity(DayOfWeek.Tuesday).ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowZeroObservedCapacity_When_Recalibrating()
    {
        // Given
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 6 } });

        // When recalibrating with observed=0 (valid, e.g. rest day)
        // weighted avg = (6*1 + 0*1) / 2 = 3, delta = 3-6 = -3, capped to -1
        model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 0, historicalWeight: 1, recentWeight: 1, maxAdjustment: 1);

        // Then
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCapDelta_When_DeltaExactlyEqualsMaxAdjustment()
    {
        // Given current=6
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 6 } });

        // When weighted avg = (6*1 + 8*1) / 2 = 7, delta = 1, maxAdjustment = 1
        // delta == maxAdjustment, should NOT be capped (> not >=)
        model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 8, historicalWeight: 1, recentWeight: 1, maxAdjustment: 1);

        // Then capacity = 6 + 1 = 7
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(7);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCapDeltaDownward_When_DeltaExactlyEqualsNegativeMaxAdjustment()
    {
        // Given current=6
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 6 } });

        // When weighted avg = (6*1 + 4*1) / 2 = 5, delta = -1, maxAdjustment = 1
        // delta == -maxAdjustment, should NOT be capped (< not <=)
        model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 4, historicalWeight: 1, recentWeight: 1, maxAdjustment: 1);

        // Then capacity = 6 + (-1) = 5
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapDelta_When_DeltaExceedsMaxAdjustmentByOne()
    {
        // Given current=6
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 6 } });

        // When weighted avg = (6*1 + 10*1) / 2 = 8, delta = 2, maxAdjustment = 1
        // delta > maxAdjustment, should cap
        model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 10, historicalWeight: 1, recentWeight: 1, maxAdjustment: 1);

        // Then capacity = 6 + 1 (capped) = 7
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(7);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapDeltaDownward_When_NegativeDeltaExceedsMaxAdjustment()
    {
        // Given current=6
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 6 } });

        // When weighted avg = (6*1 + 0*1) / 2 = 3, delta = -3, maxAdjustment = 1
        // |delta| > maxAdjustment, should cap downward
        model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 0, historicalWeight: 1, recentWeight: 1, maxAdjustment: 1);

        // Then capacity = 6 + (-1 capped) = 5
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateCorrectWeightedAverage_When_WeightsAreAsymmetric()
    {
        // Given current=10
        var model = CapacityModel.BuildFromHistory(new Dictionary<DayOfWeek, int> { { DayOfWeek.Monday, 10 } });

        // When weighted avg = (10*1 + 4*2) / (1+2) = 18/3 = 6, delta = -4, maxAdjustment = 5
        // If mutated to 4/2=2 instead of 4*2=8: (10+2)/3=4, delta=-6, capped to -5 => 5 (different!)
        model.Recalibrate(DayOfWeek.Monday, newObservedCapacity: 4, historicalWeight: 1, recentWeight: 2, maxAdjustment: 5);

        // Then capacity = 10 + (-4) = 6
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(6);
    }

    // ==========================================================================
    // Scenario 5: Weekend capacity differs from weekday capacity
    // ==========================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackDifferentCapacity_When_WeekdaysAndWeekendsHaveDifferentPatterns()
    {
        // Given completion data with different weekday and weekend patterns
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 6 },
            { DayOfWeek.Tuesday, 6 },
            { DayOfWeek.Wednesday, 6 },
            { DayOfWeek.Thursday, 6 },
            { DayOfWeek.Friday, 6 },
            { DayOfWeek.Saturday, 2 },
            { DayOfWeek.Sunday, 3 },
        };

        // When
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // Then Saturday and Sunday tracked independently
        model.GetCapacity(DayOfWeek.Saturday).ShouldBe(2);
        model.GetCapacity(DayOfWeek.Sunday).ShouldBe(3);
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(6);
    }

    // ==========================================================================
    // Scenario 6: Overcommitment warning on daily view
    // ==========================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateWarning_When_ScheduledTasksExceedCapacity()
    {
        // Given weekday capacity is 18 units (6 Normal tasks)
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Wednesday, 18 },
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When 10 Normal tasks scheduled (30 units)
        var warning = model.CheckOvercommitment(DayOfWeek.Wednesday, scheduledTaskCount: 10, scheduledUnits: 30);

        // Then
        warning.ShouldNotBeNull();
        warning.Day.ShouldBe(DayOfWeek.Wednesday);
        warning.TypicalCapacityUnits.ShouldBe(18);
        warning.ScheduledTaskCount.ShouldBe(10);
        warning.ScheduledUnits.ShouldBe(30);
        warning.Message.ShouldContain("Wednesday");
        warning.Message.ShouldContain("18");
        warning.Message.ShouldContain("10");
        warning.Message.ShouldContain("Consider reprioritising");
    }

    // ==========================================================================
    // Scenario 7: Warning when adding a task to an overcommitted day
    // ==========================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateWarning_When_AddingTaskPushesOverCapacity()
    {
        // Given weekday capacity is 18 units and already have 6 Normal tasks (18 units)
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Thursday, 18 },
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When a 7th Normal task is added (21 units total)
        var warning = model.CheckOvercommitment(DayOfWeek.Thursday, scheduledTaskCount: 7, scheduledUnits: 21);

        // Then
        warning.ShouldNotBeNull();
        warning.ScheduledTaskCount.ShouldBe(7);
        warning.ScheduledUnits.ShouldBe(21);
    }

    // ==========================================================================
    // Scenario 8: No warning when within capacity
    // ==========================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotGenerateWarning_When_ScheduledTasksWithinCapacity()
    {
        // Given weekday capacity is 18 units
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Wednesday, 18 },
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When 4 Normal tasks (12 units) scheduled
        var warning = model.CheckOvercommitment(DayOfWeek.Wednesday, scheduledTaskCount: 4, scheduledUnits: 12);

        // Then
        warning.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotGenerateWarning_When_ScheduledEqualsCapacity()
    {
        // Given
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Wednesday, 18 },
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When scheduled exactly equals capacity
        var warning = model.CheckOvercommitment(DayOfWeek.Wednesday, scheduledTaskCount: 6, scheduledUnits: 18);

        // Then
        warning.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeOvercommitted_When_ScheduledEqualsCapacity()
    {
        // Given
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 18 },
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When
        bool result = model.IsOvercommitted(DayOfWeek.Monday, 18);

        // Then
        result.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeOvercommitted_When_ScheduledExceedsCapacity()
    {
        // Given
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 18 },
        };
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // When
        bool result = model.IsOvercommitted(DayOfWeek.Monday, 19);

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowZeroCapacityInHistory_When_RestDayConfigured()
    {
        // Given
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Sunday, 0 },
        };

        // When
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // Then
        model.GetCapacity(DayOfWeek.Sunday).ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeCapacityByDay_When_ModelBuilt()
    {
        // Given
        var history = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 6 },
            { DayOfWeek.Saturday, 3 },
        };

        // When
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // Then
        model.CapacityByDay.Count.ShouldBe(2);
        model.CapacityByDay[DayOfWeek.Monday].ShouldBe(6);
        model.CapacityByDay[DayOfWeek.Saturday].ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BuildEmptyModel_When_EmptyHistoryProvided()
    {
        // Given
        var history = new Dictionary<DayOfWeek, int>();

        // When
        CapacityModel model = CapacityModel.BuildFromHistory(history);

        // Then
        model.CapacityByDay.Count.ShouldBe(0);
        model.GetCapacity(DayOfWeek.Monday).ShouldBe(0);
    }
}
