using EM2Devs.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

public sealed class StreakSnapshotConfiguration : IEntityTypeConfiguration<StreakSnapshot>
{
    public void Configure(EntityTypeBuilder<StreakSnapshot> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("streak_snapshots");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(s => s.SnapshotDate)
            .HasColumnName("snapshot_date")
            .IsRequired();

        // One snapshot per (user, day). Replaces the previous global unique index on
        // snapshot_date which assumed single-user mode.
        builder.HasIndex(s => new { s.UserId, s.SnapshotDate }).IsUnique();

        builder.Property(s => s.CurrentDays)
            .HasColumnName("current_days")
            .IsRequired();

        builder.Property(s => s.LongestDays)
            .HasColumnName("longest_days")
            .IsRequired();

        builder.Property(s => s.GraceDaysAvailable)
            .HasColumnName("grace_days_available")
            .IsRequired();

        builder.Property(s => s.WasActive)
            .HasColumnName("was_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
