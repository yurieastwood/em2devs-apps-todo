using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

public sealed class InsightCardConfiguration : IEntityTypeConfiguration<InsightCard>
{
    public void Configure(EntityTypeBuilder<InsightCard> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("insight_cards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new InsightCardId(value));

        // Shadow property — InsightCard has no UserId field on the domain entity.
        builder.Property<Guid>("UserId").HasColumnName("user_id").IsRequired();
        builder.HasIndex("UserId");

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Message)
            .HasColumnName("message")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(c => c.SupportingData)
            .HasColumnName("supporting_data")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.GeneratedAt).HasColumnName("generated_at").IsRequired();
        builder.Property(c => c.IsValidated).HasColumnName("is_validated").IsRequired();
    }
}
