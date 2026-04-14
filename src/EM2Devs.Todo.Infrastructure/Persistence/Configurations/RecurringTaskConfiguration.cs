using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

public sealed class RecurringTaskConfiguration : IEntityTypeConfiguration<RecurringTask>
{
    public void Configure(EntityTypeBuilder<RecurringTask> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("recurring_tasks");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new RecurringTaskId(value));

        builder.Property(r => r.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired()
            .HasConversion(
                title => title.Value,
                value => new TaskTitle(value));

        builder.Property(r => r.Pattern)
            .HasColumnName("pattern")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(r => r.EndDate)
            .HasColumnName("end_date");
    }
}
