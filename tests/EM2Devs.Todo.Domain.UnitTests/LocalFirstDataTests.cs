using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for local-first data ownership value objects.
/// Maps to: docs/features/data/local-first-data.feature
/// Covers: DataExportRequest, DataExportResult, SyncSettings, SyncConflict,
///         AccountDeletion, DataPrivacySettings, ExportSchedule
/// </summary>
public sealed class LocalFirstDataTests
{
    private static readonly DateTimeOffset _now = new(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);

    // ══════════════════════════════════════════════════════════════════
    // Scenario 1: App works without internet connection
    // Rule: All data is stored locally by default and the app works offline
    // Domain concept: DataPrivacySettings defaults to fully local
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToFullyLocal_When_CreatingDefaultPrivacySettings()
    {
        // Given / When
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault();

        // Then — all core features available offline means sync is off by default
        settings.SyncEnabled.ShouldBeFalse();
        settings.TelemetryEnabled.ShouldBeFalse();
        settings.IsFullyLocal.ShouldBeTrue();
        settings.HasExportSchedule.ShouldBeFalse();
        settings.ExportSchedule.ShouldBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 2: Data persists across app restarts
    // Domain concept: DataExportResult captures persisted state for verification
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackRecordCount_When_ExportingPersistedData()
    {
        // Given — 10 tasks created, 5 completed (persistence verification)
        var result = new DataExportResult("{}", DataExportFormat.Json, _now, 10);

        // Then
        result.RecordCount.ShouldBe(10);
        result.HasRecords.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportNoRecords_When_ExportIsEmpty()
    {
        // Given / When
        var result = new DataExportResult("", DataExportFormat.Json, _now, 0);

        // Then
        result.HasRecords.ShouldBeFalse();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 3: No data sent to servers without explicit opt-in
    // Domain concept: DataPrivacySettings enforces local-first default
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeFullyLocal_When_SyncAndTelemetryDisabled()
    {
        // Given
        var settings = new DataPrivacySettings(false, false, null);

        // Then — no network calls except auth/subscription
        settings.IsFullyLocal.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeFullyLocal_When_SyncIsEnabled()
    {
        // Given / When
        var settings = new DataPrivacySettings(true, false, null);

        // Then
        settings.IsFullyLocal.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeFullyLocal_When_TelemetryIsEnabled()
    {
        // Given / When
        var settings = new DataPrivacySettings(false, true, null);

        // Then
        settings.IsFullyLocal.ShouldBeFalse();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 4: Enable cross-device sync (premium)
    // Domain concept: SyncSettings — opt-in enable
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EnableSync_When_PremiumUserOptsIn()
    {
        // Given
        SyncSettings settings = SyncSettings.CreateDefault();
        settings.Enabled.ShouldBeFalse();

        // When
        SyncSettings enabled = settings.Enable();

        // Then
        enabled.Enabled.ShouldBeTrue();
        enabled.LastSyncAt.ShouldBeNull();
        enabled.ConflictResolution.ShouldBe(ConflictResolutionStrategy.LastWriteWins);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EnableSyncInPrivacySettings_When_UserConfirms()
    {
        // Given
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault();

        // When
        DataPrivacySettings updated = settings.EnableSync();

        // Then
        updated.SyncEnabled.ShouldBeTrue();
        updated.IsFullyLocal.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordSyncTimestamp_When_SyncCompletes()
    {
        // Given
        SyncSettings settings = SyncSettings.CreateDefault().Enable();

        // When
        SyncSettings synced = settings.RecordSync(_now);

        // Then
        synced.LastSyncAt.ShouldBe(_now);
        synced.Enabled.ShouldBeTrue();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 5: Sync conflict resolution (premium)
    // Domain concept: SyncConflict — last-write-wins with conflict log
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectConflict_When_BothDevicesMakeChangesOffline()
    {
        // Given — device A completes, device B edits the same task
        string localVersion = "completed on device A";
        string remoteVersion = "edited on device B";

        // When — both come online, conflict detected and resolved
        SyncConflict conflict = SyncConflict.ResolveLastWriteWins(localVersion, remoteVersion, _now);

        // Then
        conflict.LocalVersion.ShouldBe(localVersion);
        conflict.RemoteVersion.ShouldBe(remoteVersion);
        conflict.Resolution.ShouldBe(ConflictResolutionStrategy.LastWriteWins);
        conflict.ResolvedAt.ShouldBe(_now);
        conflict.IsIdentical.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectIdenticalVersions_When_NoRealConflict()
    {
        // Given / When
        var conflict = new SyncConflict("same", "same", ConflictResolutionStrategy.LastWriteWins, _now);

        // Then
        conflict.IsIdentical.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_LocalVersionIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new SyncConflict("", "remote", ConflictResolutionStrategy.LastWriteWins, _now));
        ex.Message.ShouldContain("Local version cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RemoteVersionIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new SyncConflict("local", "", ConflictResolutionStrategy.LastWriteWins, _now));
        ex.Message.ShouldContain("Remote version cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ConflictTimestampIsDefault()
    {
        var ex = Should.Throw<DomainException>(
            () => new SyncConflict("local", "remote", ConflictResolutionStrategy.LastWriteWins, default));
        ex.Message.ShouldContain("timestamp cannot be default");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ConflictResolutionStrategyIsInvalid()
    {
        var ex = Should.Throw<DomainException>(
            () => new SyncConflict("local", "remote", (ConflictResolutionStrategy)999, _now));
        ex.Message.ShouldContain("Invalid conflict resolution strategy");
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 6: Disable sync and delete cloud data (premium)
    // Domain concept: SyncSettings — disable + DataPrivacySettings
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DisableSync_When_UserOptsOut()
    {
        // Given
        SyncSettings settings = SyncSettings.CreateDefault().Enable();
        settings.Enabled.ShouldBeTrue();

        // When
        SyncSettings disabled = settings.Disable();

        // Then — local data remains, cloud data removed
        disabled.Enabled.ShouldBeFalse();
        disabled.LastSyncAt.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DisableSyncInPrivacySettings_When_UserRemovesCloudData()
    {
        // Given
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault().EnableSync();

        // When
        DataPrivacySettings updated = settings.DisableSync();

        // Then
        updated.SyncEnabled.ShouldBeFalse();
        updated.IsFullyLocal.ShouldBeTrue();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 7: Social features require server-side state
    // Domain concept: SyncSettings tracks last sync for stale data notice
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackLastSyncTimestamp_When_SyncHasOccurred()
    {
        // Given — synced previously, now offline
        SyncSettings settings = SyncSettings.CreateDefault().Enable().RecordSync(_now);

        // Then — last-synced state available for display
        settings.LastSyncAt.ShouldBe(_now);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNoLastSync_When_SyncJustEnabled()
    {
        // Given / When
        SyncSettings settings = SyncSettings.CreateDefault().Enable();

        // Then
        settings.LastSyncAt.ShouldBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 8: Export all data as JSON
    // Domain concept: DataExportRequest (JSON, All) + DataExportResult
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateJsonExportRequest_When_ExportingAllData()
    {
        // When
        DataExportRequest request = DataExportRequest.AllAsJson(_now);

        // Then
        request.Format.ShouldBe(DataExportFormat.Json);
        request.Scope.ShouldBe(DataExportScope.All);
        request.RequestedAt.ShouldBe(_now);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateExportResult_When_JsonExportCompletes()
    {
        // Given / When
        var result = new DataExportResult("{\"tasks\":[]}", DataExportFormat.Json, _now, 42);

        // Then
        result.Content.ShouldBe("{\"tasks\":[]}");
        result.Format.ShouldBe(DataExportFormat.Json);
        result.ExportedAt.ShouldBe(_now);
        result.RecordCount.ShouldBe(42);
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 9: Export tasks as CSV
    // Domain concept: DataExportRequest (CSV, TasksOnly)
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCsvExportRequest_When_ExportingTasks()
    {
        // When
        DataExportRequest request = DataExportRequest.TasksAsCsv(_now);

        // Then
        request.Format.ShouldBe(DataExportFormat.Csv);
        request.Scope.ShouldBe(DataExportScope.TasksOnly);
        request.RequestedAt.ShouldBe(_now);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCsvExportResult_When_CsvExportCompletes()
    {
        // Given / When
        var result = new DataExportResult("title,status\nTask1,Done", DataExportFormat.Csv, _now, 1);

        // Then
        result.Format.ShouldBe(DataExportFormat.Csv);
        result.RecordCount.ShouldBe(1);
        result.HasRecords.ShouldBeTrue();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 10: Export is always available regardless of subscription
    // Domain concept: DataExportRequest has no tier restriction
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowJsonExport_When_FreeTierUser()
    {
        // Given — free-tier user
        // When — both formats should be creatable without any tier check
        DataExportRequest jsonRequest = DataExportRequest.AllAsJson(_now);
        DataExportRequest csvRequest = DataExportRequest.TasksAsCsv(_now);

        // Then — no exceptions, no tier validation in domain
        jsonRequest.Format.ShouldBe(DataExportFormat.Json);
        csvRequest.Format.ShouldBe(DataExportFormat.Csv);
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 11: Import data from a previous export
    // Domain concept: DataExportResult can represent importable data
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateImportableExportResult_When_PreviousExportLoaded()
    {
        // Given — a previously exported JSON file
        string exportContent = "{\"tasks\":[{\"title\":\"Task1\"}],\"xp\":500}";
        var result = new DataExportResult(exportContent, DataExportFormat.Json, _now, 15);

        // Then — data available for import/restore
        result.Content.ShouldBe(exportContent);
        result.Format.ShouldBe(DataExportFormat.Json);
        result.RecordCount.ShouldBe(15);
        result.HasRecords.ShouldBeTrue();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 12: Scheduled automatic export (premium)
    // Domain concept: ExportSchedule + DataPrivacySettings.WithExportSchedule
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWeeklyExportSchedule_When_PremiumUserConfigures()
    {
        // When
        ExportSchedule schedule = ExportSchedule.Weekly(DayOfWeek.Sunday, "/backups/waypoint");

        // Then
        schedule.DayOfWeek.ShouldBe(DayOfWeek.Sunday);
        schedule.LocalDirectory.ShouldBe("/backups/waypoint");
        schedule.RetainedBackups.ShouldBe(ExportSchedule.MaxRetainedBackups);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RetainFourMostRecentBackups_When_ScheduleCreated()
    {
        // Given / When
        ExportSchedule schedule = ExportSchedule.Weekly(DayOfWeek.Monday, "/backups");

        // Then
        schedule.RetainedBackups.ShouldBe(4);
        ExportSchedule.MaxRetainedBackups.ShouldBe(4);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AttachExportSchedule_When_ConfiguredInPrivacySettings()
    {
        // Given
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault();
        ExportSchedule schedule = ExportSchedule.Weekly(DayOfWeek.Friday, "/exports");

        // When
        DataPrivacySettings updated = settings.WithExportSchedule(schedule);

        // Then
        updated.HasExportSchedule.ShouldBeTrue();
        updated.ExportSchedule.ShouldBe(schedule);
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 13: Scheduled export when local directory is unavailable
    // Domain concept: ExportSchedule validates directory path
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExportDirectoryIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new ExportSchedule(DayOfWeek.Monday, "", 4));
        ex.Message.ShouldContain("Local directory for export cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExportDirectoryIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(
            () => new ExportSchedule(DayOfWeek.Monday, "   ", 4));
        ex.Message.ShouldContain("Local directory for export cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveExportSchedule_When_ClearedFromPrivacySettings()
    {
        // Given
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault()
            .WithExportSchedule(ExportSchedule.Weekly(DayOfWeek.Friday, "/exports"));

        // When
        DataPrivacySettings updated = settings.WithoutExportSchedule();

        // Then
        updated.HasExportSchedule.ShouldBeFalse();
        updated.ExportSchedule.ShouldBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 14: Delete all data
    // Domain concept: DataPrivacySettings reset to default after wipe
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetToDefault_When_AllDataDeleted()
    {
        // Given — user had sync and telemetry enabled
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault()
            .EnableSync()
            .EnableTelemetry()
            .WithExportSchedule(ExportSchedule.Weekly(DayOfWeek.Monday, "/backups"));

        // When — all data deleted, account empty
        DataPrivacySettings reset = DataPrivacySettings.CreateDefault();

        // Then
        reset.SyncEnabled.ShouldBeFalse();
        reset.TelemetryEnabled.ShouldBeFalse();
        reset.IsFullyLocal.ShouldBeTrue();
        reset.HasExportSchedule.ShouldBeFalse();

        // Original should have had different state
        settings.SyncEnabled.ShouldBeTrue();
        settings.TelemetryEnabled.ShouldBeTrue();
        settings.HasExportSchedule.ShouldBeTrue();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 15: Delete account entirely
    // Domain concept: AccountDeletion with 30-day holding period
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SchedulePurgeIn30Days_When_AccountDeleted()
    {
        // When
        AccountDeletion deletion = AccountDeletion.Request(_now);

        // Then
        deletion.RequestedAt.ShouldBe(_now);
        deletion.ScheduledPurgeAt.ShouldBe(_now.AddDays(30));
        deletion.Recovered.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HoldFor30Days_When_AccountDeletionRequested()
    {
        // Given
        AccountDeletion deletion = AccountDeletion.Request(_now);

        // Then
        AccountDeletion.HoldingPeriod.ShouldBe(TimeSpan.FromDays(30));
        deletion.IsRecoverable(_now.AddDays(15)).ShouldBeTrue();
        deletion.IsPurgeOverdue(_now.AddDays(15)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BePurgeOverdue_When_HoldingPeriodExpires()
    {
        // Given
        AccountDeletion deletion = AccountDeletion.Request(_now);

        // When — 30 days later
        DateTimeOffset afterHolding = _now.AddDays(31);

        // Then
        deletion.IsPurgeOverdue(afterHolding).ShouldBeTrue();
        deletion.IsRecoverable(afterHolding).ShouldBeFalse();
    }

    // ══════════════════════════════════════════════════════════════════
    // Scenario 16: Recover account during the 30-day holding period
    // Domain concept: AccountDeletion.Recover
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecoverAccount_When_WithinHoldingPeriod()
    {
        // Given
        AccountDeletion deletion = AccountDeletion.Request(_now);

        // When — user signs in within 30 days
        AccountDeletion recovered = deletion.Recover(_now.AddDays(10));

        // Then
        recovered.Recovered.ShouldBeTrue();
        recovered.RequestedAt.ShouldBe(_now);
        recovered.ScheduledPurgeAt.ShouldBe(_now.AddDays(30));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBePurgeOverdue_When_AccountRecovered()
    {
        // Given
        AccountDeletion deletion = AccountDeletion.Request(_now);

        // When
        AccountDeletion recovered = deletion.Recover(_now.AddDays(10));

        // Then — holding period cancelled
        recovered.IsPurgeOverdue(_now.AddDays(31)).ShouldBeFalse();
        recovered.IsRecoverable(_now.AddDays(31)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecoveringAfterHoldingPeriod()
    {
        // Given
        AccountDeletion deletion = AccountDeletion.Request(_now);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => deletion.Recover(_now.AddDays(31)));
        ex.Message.ShouldContain("holding period has expired");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecoveringAlreadyRecoveredAccount()
    {
        // Given
        AccountDeletion deletion = AccountDeletion.Request(_now);
        AccountDeletion recovered = deletion.Recover(_now.AddDays(5));

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => recovered.Recover(_now.AddDays(10)));
        ex.Message.ShouldContain("already been recovered");
    }

    // ══════════════════════════════════════════════════════════════════
    // Validation tests — DataExportRequest
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExportRequestTimestampIsDefault()
    {
        var ex = Should.Throw<DomainException>(
            () => new DataExportRequest(DataExportFormat.Json, DataExportScope.All, default));
        ex.Message.ShouldContain("timestamp cannot be default");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExportRequestFormatIsInvalid()
    {
        var ex = Should.Throw<DomainException>(
            () => new DataExportRequest((DataExportFormat)999, DataExportScope.All, _now));
        ex.Message.ShouldContain("Invalid export format");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExportRequestScopeIsInvalid()
    {
        var ex = Should.Throw<DomainException>(
            () => new DataExportRequest(DataExportFormat.Json, (DataExportScope)999, _now));
        ex.Message.ShouldContain("Invalid export scope");
    }

    // ══════════════════════════════════════════════════════════════════
    // Validation tests — DataExportResult
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_ExportResultContentIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new DataExportResult(null!, DataExportFormat.Json, _now, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExportResultFormatIsInvalid()
    {
        var ex = Should.Throw<DomainException>(
            () => new DataExportResult("{}", (DataExportFormat)999, _now, 0));
        ex.Message.ShouldContain("Invalid export format");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExportResultTimestampIsDefault()
    {
        var ex = Should.Throw<DomainException>(
            () => new DataExportResult("{}", DataExportFormat.Json, default, 0));
        ex.Message.ShouldContain("timestamp cannot be default");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExportResultRecordCountIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new DataExportResult("{}", DataExportFormat.Json, _now, -1));
        ex.Message.ShouldContain("Record count cannot be negative");
    }

    // ══════════════════════════════════════════════════════════════════
    // Validation tests — SyncSettings
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SyncSettingsHasInvalidStrategy()
    {
        var ex = Should.Throw<DomainException>(
            () => new SyncSettings(false, null, (ConflictResolutionStrategy)999));
        ex.Message.ShouldContain("Invalid conflict resolution strategy");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DisabledSyncHasLastSyncTimestamp()
    {
        var ex = Should.Throw<DomainException>(
            () => new SyncSettings(false, _now, ConflictResolutionStrategy.LastWriteWins));
        ex.Message.ShouldContain("Cannot have a last sync timestamp when sync is disabled");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecordingSyncWhileDisabled()
    {
        // Given
        SyncSettings settings = SyncSettings.CreateDefault();

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => settings.RecordSync(_now));
        ex.Message.ShouldContain("Cannot record sync when sync is disabled");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecordingSyncWithDefaultTimestamp()
    {
        // Given
        SyncSettings settings = SyncSettings.CreateDefault().Enable();

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => settings.RecordSync(default));
        ex.Message.ShouldContain("Sync timestamp cannot be default");
    }

    // ══════════════════════════════════════════════════════════════════
    // Validation tests — AccountDeletion
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DeletionRequestTimestampIsDefault()
    {
        var ex = Should.Throw<DomainException>(
            () => AccountDeletion.Request(default));
        ex.Message.ShouldContain("timestamp cannot be default");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ScheduledPurgeIsBeforeRequest()
    {
        var ex = Should.Throw<DomainException>(
            () => new AccountDeletion(_now, _now.AddDays(-1), false));
        ex.Message.ShouldContain("Scheduled purge must be after");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ScheduledPurgeEqualsRequest()
    {
        var ex = Should.Throw<DomainException>(
            () => new AccountDeletion(_now, _now, false));
        ex.Message.ShouldContain("Scheduled purge must be after");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AccountDeletionRequestedAtIsDefault()
    {
        var ex = Should.Throw<DomainException>(
            () => new AccountDeletion(default, _now, false));
        ex.Message.ShouldContain("Deletion request timestamp cannot be default");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AccountDeletionScheduledPurgeIsDefault()
    {
        var ex = Should.Throw<DomainException>(
            () => new AccountDeletion(_now, default, false));
        ex.Message.ShouldContain("Scheduled purge timestamp cannot be default");
    }

    // ══════════════════════════════════════════════════════════════════
    // Validation tests — ExportSchedule
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExportScheduleDayIsInvalid()
    {
        var ex = Should.Throw<DomainException>(
            () => new ExportSchedule((DayOfWeek)999, "/backups", 4));
        ex.Message.ShouldContain("Invalid day of week");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RetainedBackupsIsZero()
    {
        var ex = Should.Throw<DomainException>(
            () => new ExportSchedule(DayOfWeek.Monday, "/backups", 0));
        ex.Message.ShouldContain("Must retain at least 1 backup");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RetainedBackupsExceedsMax()
    {
        var ex = Should.Throw<DomainException>(
            () => new ExportSchedule(DayOfWeek.Monday, "/backups", 5));
        ex.Message.ShouldContain("Cannot retain more than");
    }

    // ══════════════════════════════════════════════════════════════════
    // Validation tests — DataPrivacySettings
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_ExportScheduleIsNull()
    {
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault();

        Should.Throw<ArgumentNullException>(
            () => settings.WithExportSchedule(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EnableTelemetry_When_UserOptsIn()
    {
        // Given
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault();

        // When
        DataPrivacySettings updated = settings.EnableTelemetry();

        // Then
        updated.TelemetryEnabled.ShouldBeTrue();
        updated.IsFullyLocal.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DisableTelemetry_When_UserOptsOut()
    {
        // Given
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault().EnableTelemetry();

        // When
        DataPrivacySettings updated = settings.DisableTelemetry();

        // Then
        updated.TelemetryEnabled.ShouldBeFalse();
        updated.IsFullyLocal.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveSyncState_When_TogglingTelemetry()
    {
        // Given
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault().EnableSync();

        // When
        DataPrivacySettings updated = settings.EnableTelemetry();

        // Then
        updated.SyncEnabled.ShouldBeTrue();
        updated.TelemetryEnabled.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveTelemetryState_When_TogglingSync()
    {
        // Given
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault().EnableTelemetry();

        // When
        DataPrivacySettings updated = settings.EnableSync();

        // Then
        updated.SyncEnabled.ShouldBeTrue();
        updated.TelemetryEnabled.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveExportSchedule_When_TogglingSync()
    {
        // Given
        ExportSchedule schedule = ExportSchedule.Weekly(DayOfWeek.Friday, "/exports");
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault().WithExportSchedule(schedule);

        // When
        DataPrivacySettings updated = settings.EnableSync();

        // Then
        updated.ExportSchedule.ShouldBe(schedule);
        updated.SyncEnabled.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveExportSchedule_When_TogglingTelemetry()
    {
        // Given
        ExportSchedule schedule = ExportSchedule.Weekly(DayOfWeek.Friday, "/exports");
        DataPrivacySettings settings = DataPrivacySettings.CreateDefault().WithExportSchedule(schedule);

        // When
        DataPrivacySettings updated = settings.EnableTelemetry();

        // Then
        updated.ExportSchedule.ShouldBe(schedule);
        updated.TelemetryEnabled.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCustomExportSchedule_When_SpecificRetentionSet()
    {
        // Given / When
        var schedule = new ExportSchedule(DayOfWeek.Wednesday, "/my-backups", 2);

        // Then
        schedule.DayOfWeek.ShouldBe(DayOfWeek.Wednesday);
        schedule.LocalDirectory.ShouldBe("/my-backups");
        schedule.RetainedBackups.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BePurgeOverdue_When_ExactlyAtScheduledPurge()
    {
        // Given
        AccountDeletion deletion = AccountDeletion.Request(_now);

        // When — exactly at purge time
        DateTimeOffset exactPurge = _now.AddDays(30);

        // Then
        deletion.IsPurgeOverdue(exactPurge).ShouldBeTrue();
        deletion.IsRecoverable(exactPurge).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_FailRecovery_When_ExactlyAtScheduledPurge()
    {
        // Given
        AccountDeletion deletion = AccountDeletion.Request(_now);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => deletion.Recover(_now.AddDays(30)));
        ex.Message.ShouldContain("holding period has expired");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExportScheduleRetainedBackupsIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new ExportSchedule(DayOfWeek.Monday, "/backups", -1));
        ex.Message.ShouldContain("Must retain at least 1 backup");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowOneRetainedBackup_When_MinimumRetentionConfigured()
    {
        // Given / When — boundary: exactly 1 backup retained
        var schedule = new ExportSchedule(DayOfWeek.Monday, "/backups", 1);

        // Then
        schedule.RetainedBackups.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowEnabledSyncWithNullLastSync_When_Constructed()
    {
        // Given / When
        var settings = new SyncSettings(true, null, ConflictResolutionStrategy.LastWriteWins);

        // Then
        settings.Enabled.ShouldBeTrue();
        settings.LastSyncAt.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowEnabledSyncWithLastSync_When_Constructed()
    {
        // Given / When
        var settings = new SyncSettings(true, _now, ConflictResolutionStrategy.LastWriteWins);

        // Then
        settings.Enabled.ShouldBeTrue();
        settings.LastSyncAt.ShouldBe(_now);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateExportRequestWithAllScopes_When_ConstructedDirectly()
    {
        // Given / When
        var request = new DataExportRequest(DataExportFormat.Csv, DataExportScope.All, _now);

        // Then
        request.Format.ShouldBe(DataExportFormat.Csv);
        request.Scope.ShouldBe(DataExportScope.All);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeRecoverable_When_AlreadyRecovered()
    {
        // Given
        AccountDeletion deletion = AccountDeletion.Request(_now);
        AccountDeletion recovered = deletion.Recover(_now.AddDays(5));

        // Then — recovered accounts are neither recoverable nor purge-overdue
        recovered.IsRecoverable(_now.AddDays(10)).ShouldBeFalse();
        recovered.IsPurgeOverdue(_now.AddDays(10)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_LocalVersionIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(
            () => new SyncConflict("   ", "remote", ConflictResolutionStrategy.LastWriteWins, _now));
        ex.Message.ShouldContain("Local version cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RemoteVersionIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(
            () => new SyncConflict("local", "   ", ConflictResolutionStrategy.LastWriteWins, _now));
        ex.Message.ShouldContain("Remote version cannot be empty");
    }
}
