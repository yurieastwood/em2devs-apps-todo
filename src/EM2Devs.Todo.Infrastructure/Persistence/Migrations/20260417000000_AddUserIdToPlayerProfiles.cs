using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Slice 3 of multi-user data isolation: scope PlayerProfile by UserId.
/// Adds a required user_id column to player_profiles, enforces exactly one profile per user
/// via a unique index, and wires a FK to Users(id). Any pre-existing singleton profile row is
/// back-filled to the seed demo user id (<c>00000000-0000-0000-0000-000000000001</c>) via a
/// column DEFAULT that is dropped after the one-shot backfill so future inserts must supply
/// user_id explicitly.
/// </summary>
public partial class AddUserIdToPlayerProfiles : Migration
{
    private const string DemoUserId = "00000000-0000-0000-0000-000000000001";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<Guid>(
            name: "user_id",
            table: "player_profiles",
            type: "uuid",
            nullable: false,
            defaultValueSql: $"'{DemoUserId}'::uuid");

        // Drop the default so future inserts must supply user_id explicitly.
        migrationBuilder.Sql("ALTER TABLE player_profiles ALTER COLUMN user_id DROP DEFAULT;");

        migrationBuilder.CreateIndex(
            name: "IX_player_profiles_user_id",
            table: "player_profiles",
            column: "user_id",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_player_profiles_users_user_id",
            table: "player_profiles",
            column: "user_id",
            principalTable: "Users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropForeignKey(name: "FK_player_profiles_users_user_id", table: "player_profiles");
        migrationBuilder.DropIndex(name: "IX_player_profiles_user_id", table: "player_profiles");
        migrationBuilder.DropColumn(name: "user_id", table: "player_profiles");
    }
}
