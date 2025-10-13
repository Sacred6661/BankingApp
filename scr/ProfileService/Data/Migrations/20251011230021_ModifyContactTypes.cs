using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfileService.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifyContactTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ContactTypes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegexPattern",
                table: "ContactTypes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "ContactTypes");

            migrationBuilder.DropColumn(
                name: "RegexPattern",
                table: "ContactTypes");
        }
    }
}
