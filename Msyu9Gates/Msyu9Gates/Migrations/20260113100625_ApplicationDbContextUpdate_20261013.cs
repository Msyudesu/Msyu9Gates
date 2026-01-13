using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Msyu9Gates.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationDbContextUpdate_20261013 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DiffiutyLevel",
                table: "ChaptersDb",
                newName: "DifficultyLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DifficultyLevel",
                table: "ChaptersDb",
                newName: "DiffiutyLevel");
        }
    }
}
