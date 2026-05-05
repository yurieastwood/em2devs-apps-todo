using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

public sealed class EnergyCheckInConfiguration : IEntityTypeConfiguration<EnergyCheckIn>
{
    public void Configure(EntityTypeBuilder<EnergyCheckIn> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("energy_check_ins");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new EnergyCheckInId(value));

        // Shadow property — EnergyCheckIn has no UserId field on the domain entity.
        builder.Property<Guid>("UserId").HasColumnName("user_id").IsRequired();
        builder.HasIndex("UserId");

        builder.Property(c => c.Level)
            .HasColumnName("level")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.RecordedAt).HasColumnName("recorded_at").IsRequired();

        builder.Property(c => c.PreviousLevel)
            .HasColumnName("previous_level")
            .HasConversion<string?>()
            .HasMaxLength(20);

        builder.Property(c => c.HasFluctuated).HasColumnName("has_fluctuated").IsRequired();
    }
}
