using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddRecurringTaskInstanceFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<DateOnly>(
            name: "scheduled_date",
            table: "tasks",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "source_recurring_task_id",
            table: "tasks",
            type: "uuid",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropColumn(
            name: "scheduled_date",
            table: "tasks");

        migrationBuilder.DropColumn(
            name: "source_recurring_task_id",
            table: "tasks");
    }
}
