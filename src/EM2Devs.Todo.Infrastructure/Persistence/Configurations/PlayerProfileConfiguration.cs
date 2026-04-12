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
        });

        builder.Property(p => p.LongestStreak)
            .HasColumnName("longest_streak")
            .IsRequired();
    }
}
