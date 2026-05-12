using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Adds <c>user_id</c> to <c>streak_snapshots</c> and replaces the global unique index
/// on <c>snapshot_date</c> with a composite unique index on <c>(user_id, snapshot_date)</c>.
/// Existing rows (dev only) get the empty UUID as their UserId — the repository layer
/// will not see them under any real authenticated user's filter.
/// </summary>
public partial class UserScopeStreakSnapshot : Migration
{
    private static readonly string[] _compositeIndexColumns = ["user_id", "snapshot_date"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropIndex(
            name: "IX_streak_snapshots_snapshot_date",
            table: "streak_snapshots");

        migrationBuilder.AddColumn<Guid>(
            name: "user_id",
            table: "streak_snapshots",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateIndex(
            name: "IX_streak_snapshots_user_id_snapshot_date",
            table: "streak_snapshots",
            columns: _compositeIndexColumns,
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropIndex(
            name: "IX_streak_snapshots_user_id_snapshot_date",
            table: "streak_snapshots");

        migrationBuilder.DropColumn(name: "user_id", table: "streak_snapshots");

        migrationBuilder.CreateIndex(
            name: "IX_streak_snapshots_snapshot_date",
            table: "streak_snapshots",
            column: "snapshot_date",
            unique: true);
    }
}
