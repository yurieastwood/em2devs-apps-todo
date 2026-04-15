using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Slice B: persist the active streak freeze across restarts.
/// Adds two nullable columns to <c>player_profiles</c> that map to the owned
/// <c>StreakFreeze</c> value object on the <c>Streak</c> owned type. When
/// <c>ActiveFreeze</c> is null both columns are null; EF materialises the
/// owned type back to null on read so <c>IsFrozen</c> returns false.
/// </summary>
public partial class AddStreakFreezeColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<DateOnly>(
            name: "streak_freeze_frozen_at",
            table: "player_profiles",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "streak_freeze_duration",
            table: "player_profiles",
            type: "integer",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropColumn(name: "streak_freeze_duration", table: "player_profiles");
        migrationBuilder.DropColumn(name: "streak_freeze_frozen_at", table: "player_profiles");
    }
}
