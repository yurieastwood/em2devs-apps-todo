using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddTaskDescription : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "tasks",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropColumn(
            name: "Description",
            table: "tasks");
    }
}
