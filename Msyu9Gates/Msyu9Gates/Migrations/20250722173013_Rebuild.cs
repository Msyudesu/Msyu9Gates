using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Msyu9Gates.Migrations
{
    /// <inheritdoc />
    public partial class Rebuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChaptersDb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GateId = table.Column<int>(type: "INTEGER", nullable: false),
                    Chapter = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateUnlocked = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateCompleted = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChaptersDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KeysDb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KeyValue = table.Column<string>(type: "TEXT", nullable: true),
                    Discovered = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateDiscovered = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeysDb", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChaptersDb");

            migrationBuilder.DropTable(
                name: "KeysDb");
        }
    }
}
