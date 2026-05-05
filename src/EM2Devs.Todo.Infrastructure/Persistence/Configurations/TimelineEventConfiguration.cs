using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

public sealed class TimelineEventConfiguration : IEntityTypeConfiguration<TimelineEvent>
{
    public void Configure(EntityTypeBuilder<TimelineEvent> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("timeline_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new TimelineEventId(value));

        // Shadow property — TimelineEvent has no UserId field on the domain entity.
        builder.Property<Guid>("UserId").HasColumnName("user_id").IsRequired();
        builder.HasIndex("UserId");

        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.OccurredAt).HasColumnName("occurred_at").IsRequired();

        builder.Property(e => e.Details)
            .HasColumnName("details")
            .HasMaxLength(2000)
            .IsRequired();

        // PersonalNote is a value object with two fields — store inline as nullable owned entity.
        builder.OwnsOne(e => e.Note, note =>
        {
            note.Property(n => n.Text)
                .HasColumnName("note_text")
                .HasMaxLength(PersonalNote.MaxLength);

            note.Property(n => n.CreatedAt)
                .HasColumnName("note_created_at");
        });
    }
}
