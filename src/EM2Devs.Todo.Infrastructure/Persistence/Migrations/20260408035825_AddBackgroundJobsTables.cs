using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddBackgroundJobsTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.CreateTable(
            name: "player_profiles",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                level_value = table.Column<int>(type: "integer", nullable: false),
                level_current_xp = table.Column<int>(type: "integer", nullable: false),
                streak_current_days = table.Column<int>(type: "integer", nullable: false),
                streak_last_active_date = table.Column<DateOnly>(type: "date", nullable: true),
                streak_grace_days_available = table.Column<int>(type: "integer", nullable: false),
                longest_streak = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_player_profiles", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "recurring_tasks",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                pattern = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                last_generated_at = table.Column<DateOnly>(type: "date", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_recurring_tasks", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "streak_snapshots",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                snapshot_date = table.Column<DateOnly>(type: "date", nullable: false),
                current_days = table.Column<int>(type: "integer", nullable: false),
                longest_days = table.Column<int>(type: "integer", nullable: false),
                grace_days_available = table.Column<int>(type: "integer", nullable: false),
                was_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_streak_snapshots", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_streak_snapshots_snapshot_date",
            table: "streak_snapshots",
            column: "snapshot_date",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropTable(name: "player_profiles");
        migrationBuilder.DropTable(name: "recurring_tasks");
        migrationBuilder.DropTable(name: "streak_snapshots");
    }
}
