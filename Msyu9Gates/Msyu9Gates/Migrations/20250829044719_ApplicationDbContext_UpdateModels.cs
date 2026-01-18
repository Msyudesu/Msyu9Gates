using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Msyu9Gates.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationDbContext_UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDiscovered",
                table: "KeysDb");

            migrationBuilder.RenameColumn(
                name: "DateUnlocked",
                table: "ChaptersDb",
                newName: "RouteGuid");

            migrationBuilder.RenameColumn(
                name: "DateCompleted",
                table: "ChaptersDb",
                newName: "Narrative");

            migrationBuilder.RenameColumn(
                name: "Chapter",
                table: "ChaptersDb",
                newName: "DiffiutyLevel");

            migrationBuilder.AddColumn<int>(
                name: "ChapterId",
                table: "KeysDb",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChapterModelId",
                table: "KeysDb",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateDiscoveredUtc",
                table: "KeysDb",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GateId",
                table: "KeysDb",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KeyNumber",
                table: "KeysDb",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChapterNumber",
                table: "ChaptersDb",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateCompletedUtc",
                table: "ChaptersDb",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateUnlockedUtc",
                table: "ChaptersDb",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GateModelId",
                table: "ChaptersDb",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttemptsDb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttemptedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    GateId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    AttemptValue = table.Column<string>(type: "TEXT", nullable: true),
                    ChapterModelId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttemptsDb", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttemptsDb_ChaptersDb_ChapterModelId",
                        column: x => x.ChapterModelId,
                        principalTable: "ChaptersDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GatesDb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GateNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    GateOverallDifficultyLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateUnlocked = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DateCompleted = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Narrative = table.Column<string>(type: "TEXT", nullable: true),
                    Conclusion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatesDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersDb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscordId = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Avatar = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDateUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastLoginUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersDb", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeysDb_ChapterModelId",
                table: "KeysDb",
                column: "ChapterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_KeysDb_KeyNumber_KeyValue",
                table: "KeysDb",
                columns: new[] { "KeyNumber", "KeyValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChaptersDb_GateId_ChapterNumber",
                table: "ChaptersDb",
                columns: new[] { "GateId", "ChapterNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChaptersDb_GateModelId",
                table: "ChaptersDb",
                column: "GateModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AttemptsDb_ChapterModelId",
                table: "AttemptsDb",
                column: "ChapterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AttemptsDb_UserId_GateId_ChapterId",
                table: "AttemptsDb",
                columns: new[] { "UserId", "GateId", "ChapterId" });

            migrationBuilder.CreateIndex(
                name: "IX_GatesDb_GateNumber",
                table: "GatesDb",
                column: "GateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersDb_DiscordId",
                table: "UsersDb",
                column: "DiscordId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChaptersDb_GatesDb_GateModelId",
                table: "ChaptersDb",
                column: "GateModelId",
                principalTable: "GatesDb",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KeysDb_ChaptersDb_ChapterModelId",
                table: "KeysDb",
                column: "ChapterModelId",
                principalTable: "ChaptersDb",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChaptersDb_GatesDb_GateModelId",
                table: "ChaptersDb");

            migrationBuilder.DropForeignKey(
                name: "FK_KeysDb_ChaptersDb_ChapterModelId",
                table: "KeysDb");

            migrationBuilder.DropTable(
                name: "AttemptsDb");

            migrationBuilder.DropTable(
                name: "GatesDb");

            migrationBuilder.DropTable(
                name: "UsersDb");

            migrationBuilder.DropIndex(
                name: "IX_KeysDb_ChapterModelId",
                table: "KeysDb");

            migrationBuilder.DropIndex(
                name: "IX_KeysDb_KeyNumber_KeyValue",
                table: "KeysDb");

            migrationBuilder.DropIndex(
                name: "IX_ChaptersDb_GateId_ChapterNumber",
                table: "ChaptersDb");

            migrationBuilder.DropIndex(
                name: "IX_ChaptersDb_GateModelId",
                table: "ChaptersDb");

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "KeysDb");

            migrationBuilder.DropColumn(
                name: "ChapterModelId",
                table: "KeysDb");

            migrationBuilder.DropColumn(
                name: "DateDiscoveredUtc",
                table: "KeysDb");

            migrationBuilder.DropColumn(
                name: "GateId",
                table: "KeysDb");

            migrationBuilder.DropColumn(
                name: "KeyNumber",
                table: "KeysDb");

            migrationBuilder.DropColumn(
                name: "ChapterNumber",
                table: "ChaptersDb");

            migrationBuilder.DropColumn(
                name: "DateCompletedUtc",
                table: "ChaptersDb");

            migrationBuilder.DropColumn(
                name: "DateUnlockedUtc",
                table: "ChaptersDb");

            migrationBuilder.DropColumn(
                name: "GateModelId",
                table: "ChaptersDb");

            migrationBuilder.RenameColumn(
                name: "RouteGuid",
                table: "ChaptersDb",
                newName: "DateUnlocked");

            migrationBuilder.RenameColumn(
                name: "Narrative",
                table: "ChaptersDb",
                newName: "DateCompleted");

            migrationBuilder.RenameColumn(
                name: "DiffiutyLevel",
                table: "ChaptersDb",
                newName: "Chapter");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDiscovered",
                table: "KeysDb",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
