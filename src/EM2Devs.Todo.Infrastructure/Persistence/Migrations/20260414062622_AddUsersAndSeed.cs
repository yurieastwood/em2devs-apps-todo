using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EM2Devs.Todo.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddUsersAndSeed : Migration
{
    // Pre-computed BCrypt hashes of the dev seed password "demo1234".
    // Hashes are non-deterministic, so they must be baked into the migration
    // rather than computed at design-time.
    private const string DemoUserPasswordHash =
        "$2a$11$YZwQAxRQqwTywrG02aTfZubS8CQlMYNZ8LDdbazTwpr/sOzDLexBK";

    private const string Demo2UserPasswordHash =
        "$2a$11$bmZiduSEJFqgI.CBUQJXI.4JdGItNc/UvU5eN2V/PLkJDXEod/Fzq";

    private static readonly Guid _demoUserId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid _demo2UserId = new("00000000-0000-0000-0000-000000000002");
    private static readonly DateTimeOffset _seedCreatedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly string[] _seedColumns =
    [
        "id", "email", "password_hash", "display_name", "created_at",
    ];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(
                    type: "character varying(254)",
                    maxLength: 254,
                    nullable: false),
                password_hash = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false),
                display_name = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.id));

        migrationBuilder.CreateIndex(
            name: "IX_Users_email",
            table: "Users",
            column: "email",
            unique: true);

        migrationBuilder.InsertData(
            table: "Users",
            columns: _seedColumns,
            values: new object[,]
            {
                {
                    _demoUserId,
                    "demo@waypoint.dev",
                    DemoUserPasswordHash,
                    "Demo User",
                    _seedCreatedAt,
                },
                {
                    _demo2UserId,
                    "demo2@waypoint.dev",
                    Demo2UserPasswordHash,
                    "Demo User 2",
                    _seedCreatedAt,
                },
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropTable(name: "Users");
    }
}
