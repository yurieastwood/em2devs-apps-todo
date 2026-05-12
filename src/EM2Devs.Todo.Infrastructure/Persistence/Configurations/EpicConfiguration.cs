using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

public sealed class EpicConfiguration : IEntityTypeConfiguration<Epic>
{
    public void Configure(EntityTypeBuilder<Epic> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("epics");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new EpicId(value));

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired()
            .HasConversion(t => t.Value, value => new EpicTitle(value));

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.TargetDate).HasColumnName("target_date");

        builder.Property(e => e.IsCompleted)
            .HasColumnName("is_completed")
            .IsRequired();

        builder.Property(e => e.SagaId)
            .HasColumnName("saga_id")
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new SagaId(value.Value) : null);

        builder.Ignore(e => e.Quests);
        builder.Ignore(e => e.Progress);

        // Multi-user isolation: every epic is owned by exactly one user. Modelled as a
        // shadow property since the Epic aggregate doesn't expose UserId directly. The
        // column already exists in the Postgres schema (see AddQuestEpicReflectionInsightEnergyTimeline).
        builder.Property<Guid>("UserId").HasColumnName("user_id").IsRequired();
        builder.HasIndex("UserId");
    }
}
