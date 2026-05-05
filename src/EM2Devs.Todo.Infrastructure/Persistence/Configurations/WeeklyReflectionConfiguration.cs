using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EM2Devs.Todo.Infrastructure.Persistence.Configurations;

// Internal EF row type. The domain-facing API is WeeklyReflectionReadModel
// (a flat record); this row exists only to give EF Core a stable shape for
// the weekly_reflections table. Mapped both ways inside PostgresWeeklyReflectionRepository.
internal sealed class WeeklyReflectionRow
{
    public Guid UserId { get; set; }
    public DateOnly WeekOf { get; set; }
    public string WhatWentWell { get; set; } = string.Empty;
    public string WhatDragged { get; set; } = string.Empty;
    public string Adjustment { get; set; } = string.Empty;
    public DateTimeOffset SavedAt { get; set; }
}

internal sealed class WeeklyReflectionConfiguration : IEntityTypeConfiguration<WeeklyReflectionRow>
{
    public void Configure(EntityTypeBuilder<WeeklyReflectionRow> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("weekly_reflections");
        builder.HasKey(r => new { r.UserId, r.WeekOf });

        builder.Property(r => r.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(r => r.WeekOf).HasColumnName("week_of").IsRequired();
        builder.Property(r => r.WhatWentWell).HasColumnName("what_went_well").IsRequired();
        builder.Property(r => r.WhatDragged).HasColumnName("what_dragged").IsRequired();
        builder.Property(r => r.Adjustment).HasColumnName("adjustment").IsRequired();
        builder.Property(r => r.SavedAt).HasColumnName("saved_at").IsRequired();
    }
}
