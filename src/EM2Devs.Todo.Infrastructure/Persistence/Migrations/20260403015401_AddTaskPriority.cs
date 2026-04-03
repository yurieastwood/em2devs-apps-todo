using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddTaskPriority : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<string>(
            name: "priority",
            table: "tasks",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Medium");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropColumn(
            name: "priority",
            table: "tasks");
    }
}
