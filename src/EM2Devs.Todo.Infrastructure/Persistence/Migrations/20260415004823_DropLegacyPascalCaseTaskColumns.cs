using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Drops the legacy PascalCase columns on `tasks` that were created by the
/// InitialCreate migration when TodoTaskConfiguration had no explicit
/// HasColumnName for Difficulty / DueDate / Description / CompletedAt / IsBossTask.
/// AddPhase0to3TaskColumns created snake_case duplicates that are now the
/// mapped columns; the PascalCase ones are stale duplicates that fail
/// inserts because EF no longer populates them.
/// </summary>
public partial class DropLegacyPascalCaseTaskColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropColumn(name: "IsBossTask", table: "tasks");
        migrationBuilder.DropColumn(name: "Difficulty", table: "tasks");
        migrationBuilder.DropColumn(name: "DueDate", table: "tasks");
        migrationBuilder.DropColumn(name: "Description", table: "tasks");
        migrationBuilder.DropColumn(name: "CompletedAt", table: "tasks");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<bool>(
            name: "IsBossTask", table: "tasks", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>(
            name: "Difficulty", table: "tasks", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "DueDate", table: "tasks", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "Description", table: "tasks", type: "text", nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CompletedAt", table: "tasks", type: "timestamp with time zone", nullable: true);
    }
}
