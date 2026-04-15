using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

public sealed class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("player_profiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new PlayerProfileId(value));

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // Slice 3: per-user profile isolation. One PlayerProfile per User, enforced by the unique
        // index on user_id. Concurrent create-on-first-request races arbitrated by this index.
        builder.HasIndex(p => p.UserId).IsUnique();

        // Level value object — flattened to two columns
        builder.OwnsOne(p => p.Level, level =>
        {
            level.Property(l => l.Value)
                .HasColumnName("level_value")
                .IsRequired();

            level.OwnsOne(l => l.CurrentXp, xp =>
            {
                xp.Property(x => x.Value)
                    .HasColumnName("level_current_xp")
                    .IsRequired();
            });
        });

        // Streak value object — flattened to three columns
        builder.OwnsOne(p => p.Streak, streak =>
        {
            streak.Property(s => s.CurrentDays)
                .HasColumnName("streak_current_days")
                .IsRequired();

            streak.Property(s => s.LastActiveDate)
                .HasColumnName("streak_last_active_date");

            streak.Property(s => s.GraceDaysAvailable)
                .HasColumnName("streak_grace_days_available")
                .IsRequired();

            // ActiveFreeze (Phase 1 streak freeze feature) is transient — not persisted
            // across restarts. Domain reconstructs it via Freeze()/Unfreeze() within a session.
            streak.Ignore(s => s.ActiveFreeze);
            streak.Ignore(s => s.IsFrozen);
        });

        builder.Property(p => p.LongestStreak)
            .HasColumnName("longest_streak")
            .IsRequired();

        // Phase 3 collections — currently in-memory only; the dashboard projects them
        // from PlayerProfile when AwardXp / RecordSkillTreeProgress / AwardTitle are
        // called, but they are not persisted between API restarts. Persisting these
        // would require owned-collection mappings (separate slice of work).
        builder.Ignore(p => p.SkillTrees);
        builder.Ignore(p => p.TitleInventory);
        builder.Ignore(p => p.XpHistory);
    }
}
