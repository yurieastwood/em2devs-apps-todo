using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Persists the Phase 3 collections that were previously ignored by EF:
/// task tags, player XP history, earned titles (plus active-title column),
/// and skill tree progression. Four new tables are created with cascade
/// foreign keys to their aggregate roots; <c>active_title</c> is added
/// as a nullable enum column on <c>player_profiles</c>.
/// </summary>
public partial class AddOwnedCollectionsPersistence : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<string>(
            name: "active_title",
            table: "player_profiles",
            type: "character varying(40)",
            maxLength: 40,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "player_profile_skill_trees",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                current_tier = table.Column<int>(type: "integer", nullable: false),
                tasks_completed_in_tier = table.Column<int>(type: "integer", nullable: false),
                player_profile_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_player_profile_skill_trees", x => x.Id);
                table.ForeignKey(
                    name: "FK_player_profile_skill_trees_player_profiles_player_profile_id",
                    column: x => x.player_profile_id,
                    principalTable: "player_profiles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "player_profile_titles",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                earned_on = table.Column<DateOnly>(type: "date", nullable: false),
                player_profile_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_player_profile_titles", x => x.Id);
                table.ForeignKey(
                    name: "FK_player_profile_titles_player_profiles_player_profile_id",
                    column: x => x.player_profile_id,
                    principalTable: "player_profiles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "player_profile_xp_history",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                date = table.Column<DateOnly>(type: "date", nullable: false),
                xp_earned = table.Column<int>(type: "integer", nullable: false),
                source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                cumulative_total = table.Column<int>(type: "integer", nullable: false),
                player_profile_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_player_profile_xp_history", x => x.Id);
                table.ForeignKey(
                    name: "FK_player_profile_xp_history_player_profiles_player_profile_id",
                    column: x => x.player_profile_id,
                    principalTable: "player_profiles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "task_tags",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                task_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_task_tags", x => x.Id);
                table.ForeignKey(
                    name: "FK_task_tags_tasks_task_id",
                    column: x => x.task_id,
                    principalTable: "tasks",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_player_profile_skill_trees_player_profile_id",
            table: "player_profile_skill_trees",
            column: "player_profile_id");

        migrationBuilder.CreateIndex(
            name: "IX_player_profile_titles_player_profile_id",
            table: "player_profile_titles",
            column: "player_profile_id");

        migrationBuilder.CreateIndex(
            name: "IX_player_profile_xp_history_player_profile_id",
            table: "player_profile_xp_history",
            column: "player_profile_id");

        migrationBuilder.CreateIndex(
            name: "IX_task_tags_task_id",
            table: "task_tags",
            column: "task_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropTable(
            name: "player_profile_skill_trees");

        migrationBuilder.DropTable(
            name: "player_profile_titles");

        migrationBuilder.DropTable(
            name: "player_profile_xp_history");

        migrationBuilder.DropTable(
            name: "task_tags");

        migrationBuilder.DropColumn(
            name: "active_title",
            table: "player_profiles");
    }
}
