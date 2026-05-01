using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfileService.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProfileFirstCopleteAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ProfileFirstCoplete",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileFirstCoplete",
                table: "Profiles");
        }
    }
}
