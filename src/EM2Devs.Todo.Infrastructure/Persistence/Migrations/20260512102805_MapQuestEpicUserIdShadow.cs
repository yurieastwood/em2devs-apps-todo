using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Adds the <c>user_id</c> shadow column + index to the <c>quests</c> and <c>epics</c>
/// tables so the repositories can enforce per-user isolation. Existing rows (dev only —
/// no production data) default to the empty UUID; the repository layer treats such rows
/// as inaccessible.
/// </summary>
public partial class MapQuestEpicUserIdShadow : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<Guid>(
            name: "user_id",
            table: "quests",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "user_id",
            table: "epics",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateIndex(
            name: "IX_quests_user_id",
            table: "quests",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_epics_user_id",
            table: "epics",
            column: "user_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropIndex(name: "IX_quests_user_id", table: "quests");
        migrationBuilder.DropIndex(name: "IX_epics_user_id", table: "epics");
        migrationBuilder.DropColumn(name: "user_id", table: "quests");
        migrationBuilder.DropColumn(name: "user_id", table: "epics");
    }
}
