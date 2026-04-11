using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddUniqueIndexRecurringTaskScheduledDate : Migration
{
    private static readonly string[] _indexColumns = ["source_recurring_task_id", "scheduled_date"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.CreateIndex(
            name: "IX_tasks_source_recurring_task_id_scheduled_date",
            table: "tasks",
            columns: _indexColumns,
            unique: true,
            filter: "source_recurring_task_id IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropIndex(
            name: "IX_tasks_source_recurring_task_id_scheduled_date",
            table: "tasks");
    }
}
