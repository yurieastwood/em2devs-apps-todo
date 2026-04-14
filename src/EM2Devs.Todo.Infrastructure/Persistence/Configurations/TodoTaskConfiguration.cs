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
            .HasDefaultValue(TaskPriority.Medium)
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
                .HasConversion<string?>()
                .HasMaxLength(40);
        });
    }
}
