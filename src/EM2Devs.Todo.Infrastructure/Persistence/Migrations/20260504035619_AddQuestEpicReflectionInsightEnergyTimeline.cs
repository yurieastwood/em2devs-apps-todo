using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Adds the six new Postgres tables introduced in the postgres-persistence-equalize work:
/// <c>quests</c>, <c>epics</c>, <c>weekly_reflections</c>, <c>insight_cards</c>,
/// <c>energy_check_ins</c>, and <c>timeline_events</c>.
/// <c>weekly_reflections</c> uses a composite PK of <c>(user_id, week_of)</c>.
/// <c>insight_cards</c>, <c>energy_check_ins</c>, and <c>timeline_events</c> are user-scoped
/// and have an index on <c>user_id</c>.
/// </summary>
public partial class AddQuestEpicReflectionInsightEnergyTimeline : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.CreateTable(
            name: "energy_check_ins",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                previous_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                has_fluctuated = table.Column<bool>(type: "boolean", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_energy_check_ins", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "epics",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                target_date = table.Column<DateOnly>(type: "date", nullable: true),
                is_completed = table.Column<bool>(type: "boolean", nullable: false),
                saga_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_epics", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "insight_cards",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                supporting_data = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                generated_at = table.Column<DateOnly>(type: "date", nullable: false),
                is_validated = table.Column<bool>(type: "boolean", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_insight_cards", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "quests",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                due_date = table.Column<DateOnly>(type: "date", nullable: true),
                is_completed = table.Column<bool>(type: "boolean", nullable: false),
                epic_id = table.Column<Guid>(type: "uuid", nullable: true),
                total_xp_earned = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_quests", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "timeline_events",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                note_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                note_created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_timeline_events", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "weekly_reflections",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                week_of = table.Column<DateOnly>(type: "date", nullable: false),
                what_went_well = table.Column<string>(type: "text", nullable: false),
                what_dragged = table.Column<string>(type: "text", nullable: false),
                adjustment = table.Column<string>(type: "text", nullable: false),
                saved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_weekly_reflections", x => new { x.user_id, x.week_of });
            });

        migrationBuilder.CreateIndex(
            name: "IX_energy_check_ins_user_id",
            table: "energy_check_ins",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_insight_cards_user_id",
            table: "insight_cards",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_timeline_events_user_id",
            table: "timeline_events",
            column: "user_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropTable(
            name: "energy_check_ins");

        migrationBuilder.DropTable(
            name: "epics");

        migrationBuilder.DropTable(
            name: "insight_cards");

        migrationBuilder.DropTable(
            name: "quests");

        migrationBuilder.DropTable(
            name: "timeline_events");

        migrationBuilder.DropTable(
            name: "weekly_reflections");
    }
}
