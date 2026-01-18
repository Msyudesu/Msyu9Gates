using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Msyu9Gates.Migrations
{
    /// <inheritdoc />
    public partial class SchemaRenaming20260114 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttemptsDb_ChaptersDb_ChapterModelId",
                table: "AttemptsDb");

            migrationBuilder.DropForeignKey(
                name: "FK_ChaptersDb_GatesDb_GateModelId",
                table: "ChaptersDb");

            migrationBuilder.DropForeignKey(
                name: "FK_KeysDb_ChaptersDb_ChapterModelId",
                table: "KeysDb");

            migrationBuilder.DropIndex(
                name: "IX_KeysDb_ChapterModelId",
                table: "KeysDb");

            migrationBuilder.DropIndex(
                name: "IX_ChaptersDb_GateModelId",
                table: "ChaptersDb");

            migrationBuilder.DropIndex(
                name: "IX_AttemptsDb_ChapterModelId",
                table: "AttemptsDb");

            migrationBuilder.DropColumn(
                name: "ChapterModelId",
                table: "KeysDb");

            migrationBuilder.DropColumn(
                name: "GateModelId",
                table: "ChaptersDb");

            migrationBuilder.DropColumn(
                name: "ChapterModelId",
                table: "AttemptsDb");

            migrationBuilder.CreateIndex(
                name: "IX_KeysDb_ChapterId",
                table: "KeysDb",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_AttemptsDb_ChapterId",
                table: "AttemptsDb",
                column: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttemptsDb_ChaptersDb_ChapterId",
                table: "AttemptsDb",
                column: "ChapterId",
                principalTable: "ChaptersDb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChaptersDb_GatesDb_GateId",
                table: "ChaptersDb",
                column: "GateId",
                principalTable: "GatesDb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KeysDb_ChaptersDb_ChapterId",
                table: "KeysDb",
                column: "ChapterId",
                principalTable: "ChaptersDb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttemptsDb_ChaptersDb_ChapterId",
                table: "AttemptsDb");

            migrationBuilder.DropForeignKey(
                name: "FK_ChaptersDb_GatesDb_GateId",
                table: "ChaptersDb");

            migrationBuilder.DropForeignKey(
                name: "FK_KeysDb_ChaptersDb_ChapterId",
                table: "KeysDb");

            migrationBuilder.DropIndex(
                name: "IX_KeysDb_ChapterId",
                table: "KeysDb");

            migrationBuilder.DropIndex(
                name: "IX_AttemptsDb_ChapterId",
                table: "AttemptsDb");

            migrationBuilder.AddColumn<int>(
                name: "ChapterModelId",
                table: "KeysDb",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GateModelId",
                table: "ChaptersDb",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChapterModelId",
                table: "AttemptsDb",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KeysDb_ChapterModelId",
                table: "KeysDb",
                column: "ChapterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChaptersDb_GateModelId",
                table: "ChaptersDb",
                column: "GateModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AttemptsDb_ChapterModelId",
                table: "AttemptsDb",
                column: "ChapterModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttemptsDb_ChaptersDb_ChapterModelId",
                table: "AttemptsDb",
                column: "ChapterModelId",
                principalTable: "ChaptersDb",
                principalColumn: "Id");

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
    }
}
