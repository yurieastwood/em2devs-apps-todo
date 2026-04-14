using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddActualTimeRecordToTask : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.CreateTable(
            name: "task_actual_time_records",
            columns: table => new
            {
                task_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table.Column<Guid>(type: "uuid", nullable: false),
                estimated_minutes = table.Column<int>(type: "integer", nullable: false),
                actual_minutes = table.Column<int>(type: "integer", nullable: false),
                variance_percent = table.Column<double>(type: "double precision", nullable: false),
                category = table.Column<string>(
                    type: "character varying(40)",
                    maxLength: 40,
                    nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_task_actual_time_records", x => x.task_id);
                table.ForeignKey(
                    name: "FK_task_actual_time_records_tasks_task_id",
                    column: x => x.task_id,
                    principalTable: "tasks",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropTable(name: "task_actual_time_records");
    }
}
