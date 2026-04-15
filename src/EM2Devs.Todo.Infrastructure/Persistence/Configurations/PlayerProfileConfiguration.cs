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

            // ActiveFreeze persisted as two nullable columns. When null (no active freeze)
            // both columns are null; EF materialises the owned type back to null on read.
            streak.OwnsOne(s => s.ActiveFreeze, freeze =>
            {
                freeze.Property(f => f.FrozenAt)
                    .HasColumnName("streak_freeze_frozen_at");

                freeze.Property(f => f.Duration)
                    .HasColumnName("streak_freeze_duration");
            });

            // IsFrozen is computed from ActiveFreeze on the domain type.
            streak.Ignore(s => s.IsFrozen);
        });

        builder.Property(p => p.LongestStreak)
            .HasColumnName("longest_streak")
            .IsRequired();

        // Phase 3 collections — owned-collection mappings so XP history, titles,
        // and skill trees survive API restarts.

        // XpHistory: wrapper owned type with a nested owned collection of entries.
        // The XpHistory value object itself adds no columns to player_profiles;
        // its entries live in their own table.
        builder.OwnsOne(p => p.XpHistory, history =>
        {
            history.OwnsMany<XpHistoryEntry>("Entries", entries =>
            {
                entries.ToTable("player_profile_xp_history");
                entries.WithOwner().HasForeignKey("player_profile_id");
                entries.Property<int>("Id").ValueGeneratedOnAdd();
                entries.HasKey("Id");

                entries.Property(e => e.Date)
                    .HasColumnName("date")
                    .IsRequired();

                entries.Property(e => e.XpEarned)
                    .HasColumnName("xp_earned")
                    .HasConversion(xp => xp.Value, v => new ExperiencePoints(v))
                    .IsRequired();

                entries.Property(e => e.Source)
                    .HasColumnName("source")
                    .HasMaxLength(200)
                    .IsRequired();

                entries.Property(e => e.CumulativeTotal)
                    .HasColumnName("cumulative_total")
                    .HasConversion(xp => xp.Value, v => new ExperiencePoints(v))
                    .IsRequired();
            });

            history.Navigation("Entries")
                .HasField("_entries")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(p => p.XpHistory).IsRequired();

        // TitleInventory: flattens ActiveTitle to a column and owns a collection
        // of earned titles in a separate table.
        builder.OwnsOne(p => p.TitleInventory, inventory =>
        {
            inventory.Property(i => i.ActiveTitle)
                .HasColumnName("active_title")
                .HasConversion<string?>()
                .HasMaxLength(40);

            inventory.OwnsMany<Title>("EarnedTitles", titles =>
            {
                titles.ToTable("player_profile_titles");
                titles.WithOwner().HasForeignKey("player_profile_id");
                titles.Property<int>("Id").ValueGeneratedOnAdd();
                titles.HasKey("Id");

                titles.Property(t => t.Type)
                    .HasColumnName("type")
                    .HasConversion<string>()
                    .HasMaxLength(40)
                    .IsRequired();

                titles.Property(t => t.EarnedOn)
                    .HasColumnName("earned_on")
                    .IsRequired();
            });

            inventory.Navigation("EarnedTitles")
                .HasField("_earnedTitles")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(p => p.TitleInventory).IsRequired();

        // SkillTrees: owned collection on PlayerProfile accessed via the
        // private _skillTrees backing field.
        builder.OwnsMany<SkillTree>("SkillTrees", trees =>
        {
            trees.ToTable("player_profile_skill_trees");
            trees.WithOwner().HasForeignKey("player_profile_id");
            trees.Property<int>("Id").ValueGeneratedOnAdd();
            trees.HasKey("Id");

            trees.Property(t => t.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            trees.Property(t => t.CurrentTier)
                .HasColumnName("current_tier")
                .HasConversion(tier => tier.Value, v => new SkillTier(v))
                .IsRequired();

            trees.Property(t => t.TasksCompletedInTier)
                .HasColumnName("tasks_completed_in_tier")
                .IsRequired();
        });

        builder.Metadata.FindNavigation("SkillTrees")!
            .SetField("_skillTrees");
        builder.Metadata.FindNavigation("SkillTrees")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
