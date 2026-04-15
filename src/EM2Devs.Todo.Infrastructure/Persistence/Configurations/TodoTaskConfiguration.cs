using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

public sealed class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new TaskId(value));

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(t => t.UserId);

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired()
            .HasConversion(
                title => title.Value,
                value => new TaskTitle(value));

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.EstimatedTime)
            .HasColumnName("estimated_minutes")
            .HasConversion(
                e => e != null ? e.Minutes : (int?)null,
                v => v.HasValue ? TimeEstimate.FromMinutes(v.Value) : null);

        builder.Property(t => t.SourceRecurringTaskId)
            .HasColumnName("source_recurring_task_id")
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new RecurringTaskId(value.Value) : null);

        builder.Property(t => t.ScheduledDate)
            .HasColumnName("scheduled_date");

        // Constructor-bound properties added in Phase 0-3 work
        builder.Property(t => t.Difficulty)
            .HasColumnName("difficulty")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Other persisted properties added during Phases 0-3
        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(t => t.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(t => t.IsBossTask)
            .HasColumnName("is_boss_task")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(t => t.RescheduleCount)
            .HasColumnName("reschedule_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(t => t.ViewCount)
            .HasColumnName("view_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(t => t.WaitingReason)
            .HasColumnName("waiting_reason")
            .HasMaxLength(500);

        builder.Property(t => t.AssignedQuestId)
            .HasColumnName("assigned_quest_id")
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new QuestId(value.Value) : null);

        // Tags: owned collection persisted in a separate table, accessed via
        // the private _tags backing field.
        builder.OwnsMany<Tag>("Tags", tags =>
        {
            tags.ToTable("task_tags");
            tags.WithOwner().HasForeignKey("task_id");
            tags.Property<int>("Id").ValueGeneratedOnAdd();
            tags.HasKey("Id");

            tags.Property(t => t.Value)
                .HasColumnName("value")
                .HasMaxLength(Tag.MaxLength)
                .IsRequired();
        });

        builder.Metadata.FindNavigation("Tags")!
            .SetField("_tags");
        builder.Metadata.FindNavigation("Tags")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Domain-only collections and value objects not yet persisted
        // (kept in-memory; demo functionality unaffected)
        builder.Ignore(t => t.ProcrastinationSignals);
        builder.Ignore(t => t.CommitmentNote);
        builder.Ignore(t => t.IsOverdue);
        builder.Ignore(t => t.WasCompletedLate);

        // Prevent duplicate generated instances for the same recurring task + calendar date.
        // Filtered: only applies to rows that originated from a recurring task.
        builder.HasIndex(t => new { t.SourceRecurringTaskId, t.ScheduledDate })
            .IsUnique()
            .HasFilter("source_recurring_task_id IS NOT NULL");

        // Actual time record captured after task completion (Phase 2 - Actual Time Recording).
        builder.OwnsOne(t => t.ActualTimeRecord, record =>
        {
            record.ToTable("task_actual_time_records");
            record.WithOwner().HasForeignKey("task_id");
            record.HasKey("task_id");

            record.Property(r => r.Id)
                .HasColumnName("id")
                .HasConversion(
                    id => id.Value,
                    value => new EstimationRecordId(value));

            record.Property(r => r.Estimated)
                .HasColumnName("estimated_minutes")
                .HasConversion(
                    e => e.Minutes,
                    v => TimeEstimate.FromMinutes(v));

            record.Property(r => r.Actual)
                .HasColumnName("actual_minutes")
                .HasConversion(
                    a => a.Minutes,
                    v => TimeEstimate.FromMinutes(v));

            record.Property(r => r.VariancePercent)
                .HasColumnName("variance_percent");

            record.Property(r => r.Category)
                .HasColumnName("category")
                .HasConversion(
                    category => category != null ? category.Value : null,
                    value => value != null ? TaskCategory.From(value) : null)
                .HasMaxLength(40);
        });
    }
}
