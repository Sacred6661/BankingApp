using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfileService.Data.Migrations
{
    /// <inheritdoc />
    public partial class FirstCompleteDoneAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileFirstCoplete",
                table: "Profiles",
                newName: "FirstCompleteDone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstCompleteDone",
                table: "Profiles",
                newName: "ProfileFirstCoplete");
        }
    }
}
