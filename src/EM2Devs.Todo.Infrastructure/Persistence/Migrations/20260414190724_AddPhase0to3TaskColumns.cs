using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Adds columns for properties added during Phase 0–3 work that were never
/// persisted: TodoTask gains difficulty, due_date, created_at, description,
/// completed_at, is_boss_task, reschedule_count, view_count, waiting_reason,
/// assigned_quest_id; RecurringTask gains end_date.
/// </summary>
public partial class AddPhase0to3TaskColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        // tasks table additions
        migrationBuilder.AddColumn<string>(
            name: "difficulty",
            table: "tasks",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Normal");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "due_date",
            table: "tasks",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "created_at",
            table: "tasks",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "CURRENT_TIMESTAMP");

        migrationBuilder.AddColumn<string>(
            name: "description",
            table: "tasks",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "completed_at",
            table: "tasks",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "is_boss_task",
            table: "tasks",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "reschedule_count",
            table: "tasks",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "view_count",
            table: "tasks",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "waiting_reason",
            table: "tasks",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "assigned_quest_id",
            table: "tasks",
            type: "uuid",
            nullable: true);

        // recurring_tasks table additions
        migrationBuilder.AddColumn<DateOnly>(
            name: "end_date",
            table: "recurring_tasks",
            type: "date",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropColumn(name: "difficulty", table: "tasks");
        migrationBuilder.DropColumn(name: "due_date", table: "tasks");
        migrationBuilder.DropColumn(name: "created_at", table: "tasks");
        migrationBuilder.DropColumn(name: "description", table: "tasks");
        migrationBuilder.DropColumn(name: "completed_at", table: "tasks");
        migrationBuilder.DropColumn(name: "is_boss_task", table: "tasks");
        migrationBuilder.DropColumn(name: "reschedule_count", table: "tasks");
        migrationBuilder.DropColumn(name: "view_count", table: "tasks");
        migrationBuilder.DropColumn(name: "waiting_reason", table: "tasks");
        migrationBuilder.DropColumn(name: "assigned_quest_id", table: "tasks");
        migrationBuilder.DropColumn(name: "end_date", table: "recurring_tasks");
    }
}
