using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Tests for <see cref="TimeEstimationCalibrator"/> — the pure domain service that
/// computes the user's overall estimation bias factor (median of actual/estimated)
/// from a task history. Covers the time-estimation learning ritual loop.
/// </summary>
[Trait("Category", "Domain")]
public sealed class TimeEstimationCalibratorTests
{
    private static TodoTask TaskWithActual(int estimatedMinutes, int actualMinutes)
    {
        TodoTask task = TodoTask.Create(TestData.TestUserId, new TaskTitle("t"));
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(estimatedMinutes));
        task.MoveToInProgress();
        task.MarkAsDone();
        task.RecordActualTime(TimeEstimate.FromMinutes(actualMinutes));
        return task;
    }

    [Fact]
    public void Should_ReturnNotEnoughData_When_HistoryIsEmpty()
    {
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate([]);

        calibration.State.ShouldBe(EstimationCalibrationState.NotEnoughData);
        calibration.BiasFactor.ShouldBe(1.0);
        calibration.SampleSize.ShouldBe(0);
    }

    [Fact]
    public void Should_ReturnNotEnoughData_When_OnlyOneSampleExists()
    {
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate(
            [TaskWithActual(30, 60)]);

        calibration.State.ShouldBe(EstimationCalibrationState.NotEnoughData);
        calibration.BiasFactor.ShouldBe(1.0);
        calibration.SampleSize.ShouldBe(1);
    }

    [Fact]
    public void Should_IgnoreTasksWithoutActualTimeRecord()
    {
        TodoTask incomplete = TodoTask.Create(TestData.TestUserId, new TaskTitle("open"));
        incomplete.UpdateEstimatedTime(TimeEstimate.FromMinutes(60));

        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate(
            [incomplete, TaskWithActual(30, 45), TaskWithActual(60, 90), TaskWithActual(20, 30)]);

        calibration.SampleSize.ShouldBe(3);
        calibration.State.ShouldBe(EstimationCalibrationState.Calibrated);
    }

    [Fact]
    public void Should_ComputeMedianRatio_When_SamplesAreOddCount()
    {
        // ratios: 2.0, 1.5, 1.0 → sorted 1.0, 1.5, 2.0 → median 1.5
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate([
            TaskWithActual(30, 60),
            TaskWithActual(60, 90),
            TaskWithActual(20, 20),
        ]);

        calibration.State.ShouldBe(EstimationCalibrationState.Calibrated);
        calibration.BiasFactor.ShouldBe(1.5);
        calibration.SampleSize.ShouldBe(3);
    }

    [Fact]
    public void Should_ComputeMedianRatio_When_SamplesAreEvenCount()
    {
        // ratios: 1.0, 1.2, 1.4, 1.6 → median = (1.2 + 1.4) / 2 = 1.3
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate([
            TaskWithActual(10, 10),
            TaskWithActual(10, 12),
            TaskWithActual(10, 14),
            TaskWithActual(10, 16),
        ]);

        calibration.BiasFactor.ShouldBe(1.3);
        calibration.SampleSize.ShouldBe(4);
    }

    [Fact]
    public void Should_ClampBiasFactorAboveTwo_When_UserMassivelyUnderestimates()
    {
        // All ratios = 5.0 → median 5.0, clamped to 2.0.
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate([
            TaskWithActual(10, 50),
            TaskWithActual(10, 50),
            TaskWithActual(10, 50),
        ]);

        calibration.BiasFactor.ShouldBe(2.0);
    }

    [Fact]
    public void Should_ClampBiasFactorBelowHalf_When_UserMassivelyOverestimates()
    {
        // All ratios = 0.1 → median 0.1, clamped to 0.5.
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate([
            TaskWithActual(100, 10),
            TaskWithActual(100, 10),
            TaskWithActual(100, 10),
        ]);

        calibration.BiasFactor.ShouldBe(0.5);
    }

    [Fact]
    public void Should_ResistOutliers_When_MedianIsUsedInsteadOfMean()
    {
        // ratios: 1.0, 1.0, 1.0, 1.0, 10.0 → mean 2.8, median 1.0 (clamped from 1.0).
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate([
            TaskWithActual(10, 10),
            TaskWithActual(10, 10),
            TaskWithActual(10, 10),
            TaskWithActual(10, 10),
            TaskWithActual(10, 100),
        ]);

        calibration.BiasFactor.ShouldBe(1.0);
    }

    [Fact]
    public void Should_HonourConfigurableMinimumSampleThreshold()
    {
        // With threshold = 5, three samples are not enough.
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate(
            [TaskWithActual(10, 15), TaskWithActual(10, 15), TaskWithActual(10, 15)],
            minimumSamples: 5);

        calibration.State.ShouldBe(EstimationCalibrationState.NotEnoughData);
    }

    [Fact]
    public void Should_ReturnSortedMedian_When_RatiosAreAddedInArbitraryOrder()
    {
        // Insertion order: 1.0, 3.0, 1.2, 1.4, 1.6 → middle-by-position is 1.2 (unsorted),
        // but sorted the list is 1.0, 1.2, 1.4, 1.6, 3.0 → median is 1.4.
        // This specifically guards against a mutant that removes the sort step.
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate([
            TaskWithActual(10, 10),
            TaskWithActual(10, 30),
            TaskWithActual(10, 12),
            TaskWithActual(10, 14),
            TaskWithActual(10, 16),
        ]);

        calibration.BiasFactor.ShouldBe(1.4);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_TasksCollectionIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            TimeEstimationCalibrator.Calibrate(null!));
    }
}

/// <summary>
/// Tests for the <see cref="EstimationCalibration"/> value object — the projection
/// surfaced on the estimation-bias read model and applied to daily-brief estimates.
/// </summary>
[Trait("Category", "Domain")]
public sealed class EstimationCalibrationTests
{
    [Fact]
    public void Should_ReturnNullCalibratedMinutes_When_StateIsNotEnoughData()
    {
        EstimationCalibration calibration = EstimationCalibration.NotEnoughData(2);

        calibration.ApplyTo(30).ShouldBeNull();
        calibration.BiasFactor.ShouldBe(1.0);
    }

    [Fact]
    public void Should_ApplyBiasFactorAndRoundAwayFromZero_When_Calibrated()
    {
        EstimationCalibration calibration = EstimationCalibration.Calibrated(1.5, 10);

        calibration.ApplyTo(30).ShouldBe(45);
        calibration.ApplyTo(10).ShouldBe(15);
    }

    [Fact]
    public void Should_RoundFractionalCalibratedMinutesAwayFromZero()
    {
        EstimationCalibration calibration = EstimationCalibration.Calibrated(1.33, 10);

        // 30 * 1.33 = 39.9 → rounds to 40
        calibration.ApplyTo(30).ShouldBe(40);
    }

    [Fact]
    public void Should_NeverReturnZeroMinutes_When_BiasFactorRoundsToZero()
    {
        EstimationCalibration calibration = EstimationCalibration.Calibrated(0.5, 10);

        // 1 * 0.5 = 0.5 → rounds to 1 (floor clamp), ensuring UI never shows "0m".
        calibration.ApplyTo(1).ShouldBe(1);
    }

    [Fact]
    public void Should_ClampBiasFactorOnCreation_When_OutsideSupportedRange()
    {
        EstimationCalibration over = EstimationCalibration.Calibrated(5.0, 10);
        EstimationCalibration under = EstimationCalibration.Calibrated(0.1, 10);

        over.BiasFactor.ShouldBe(2.0);
        under.BiasFactor.ShouldBe(0.5);
    }

    [Fact]
    public void Should_RoundBiasFactorToTwoDecimalPlaces()
    {
        EstimationCalibration calibration = EstimationCalibration.Calibrated(1.23456, 10);

        calibration.BiasFactor.ShouldBe(1.23);
    }
}
