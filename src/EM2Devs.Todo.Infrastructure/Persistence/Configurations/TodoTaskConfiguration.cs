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

        builder.Property(t => t.SourceRecurringTaskId)
            .HasColumnName("source_recurring_task_id")
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new RecurringTaskId(value.Value) : null);

        builder.Property(t => t.ScheduledDate)
            .HasColumnName("scheduled_date");
    }
}
