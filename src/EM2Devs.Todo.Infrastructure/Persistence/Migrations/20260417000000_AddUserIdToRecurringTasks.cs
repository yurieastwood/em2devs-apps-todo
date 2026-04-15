using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Slice 2 of multi-user data isolation: scope RecurringTask by UserId.
/// Adds a required user_id column to recurring_tasks, indexes it, and wires a FK to Users(id).
/// Existing rows are back-filled to the seed demo user id
/// (<c>00000000-0000-0000-0000-000000000001</c>) via a column DEFAULT that is dropped
/// after the one-shot backfill so future inserts must supply user_id explicitly.
/// </summary>
public partial class AddUserIdToRecurringTasks : Migration
{
    private const string DemoUserId = "00000000-0000-0000-0000-000000000001";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<Guid>(
            name: "user_id",
            table: "recurring_tasks",
            type: "uuid",
            nullable: false,
            defaultValueSql: $"'{DemoUserId}'::uuid");

        // Drop the default so future inserts must supply user_id explicitly.
        migrationBuilder.Sql("ALTER TABLE recurring_tasks ALTER COLUMN user_id DROP DEFAULT;");

        migrationBuilder.CreateIndex(
            name: "IX_recurring_tasks_user_id",
            table: "recurring_tasks",
            column: "user_id");

        migrationBuilder.AddForeignKey(
            name: "FK_recurring_tasks_users_user_id",
            table: "recurring_tasks",
            column: "user_id",
            principalTable: "Users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropForeignKey(name: "FK_recurring_tasks_users_user_id", table: "recurring_tasks");
        migrationBuilder.DropIndex(name: "IX_recurring_tasks_user_id", table: "recurring_tasks");
        migrationBuilder.DropColumn(name: "user_id", table: "recurring_tasks");
    }
}
