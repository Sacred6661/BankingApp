using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfileService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Minor_changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileSettings_Languages_LanguageId",
                table: "ProfileSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileSettings_Timezones_TimeZoneId",
                table: "ProfileSettings");

            migrationBuilder.AlterColumn<int>(
                name: "TimeZoneId",
                table: "ProfileSettings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "LanguageId",
                table: "ProfileSettings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileSettings_Languages_LanguageId",
                table: "ProfileSettings",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileSettings_Timezones_TimeZoneId",
                table: "ProfileSettings",
                column: "TimeZoneId",
                principalTable: "Timezones",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileSettings_Languages_LanguageId",
                table: "ProfileSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileSettings_Timezones_TimeZoneId",
                table: "ProfileSettings");

            migrationBuilder.AlterColumn<int>(
                name: "TimeZoneId",
                table: "ProfileSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LanguageId",
                table: "ProfileSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileSettings_Languages_LanguageId",
                table: "ProfileSettings",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileSettings_Timezones_TimeZoneId",
                table: "ProfileSettings",
                column: "TimeZoneId",
                principalTable: "Timezones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
