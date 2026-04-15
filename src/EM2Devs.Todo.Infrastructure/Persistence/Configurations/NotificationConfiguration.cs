using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new NotificationId(value));

        builder.Property(n => n.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(n => n.UserId);

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasColumnName("message")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.ReadAt)
            .HasColumnName("read_at");

        // Auto-dismiss duration, delivery channel and deep-link are in-memory
        // concerns only — they aren't needed by the inbox read path.
        builder.Ignore(n => n.AutoDismissAfterSeconds);
        builder.Ignore(n => n.Channel);
        builder.Ignore(n => n.DeepLink);
        builder.Ignore(n => n.IsRead);
        builder.Ignore(n => n.IsDismissed);
    }
}
