using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

public sealed class QuestConfiguration : IEntityTypeConfiguration<Quest>
{
    public void Configure(EntityTypeBuilder<Quest> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("quests");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new QuestId(value));

        builder.Property(q => q.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired()
            .HasConversion(t => t.Value, value => new QuestTitle(value));

        builder.Property(q => q.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(q => q.DueDate).HasColumnName("due_date");

        builder.Property(q => q.IsCompleted)
            .HasColumnName("is_completed")
            .IsRequired();

        builder.Property(q => q.EpicId)
            .HasColumnName("epic_id")
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new EpicId(value.Value) : null);

        builder.Property(q => q.TotalXpEarned)
            .HasColumnName("total_xp_earned")
            .HasConversion(xp => xp.Value, value => new ExperiencePoints(value))
            .IsRequired();

        // Tasks are reconstituted via FK lookup on tasks.assigned_quest_id.
        // Quest's `_tasks` list and `Progress` (computed) are not persisted here.
        builder.Ignore(q => q.Tasks);
        builder.Ignore(q => q.Progress);
    }
}
