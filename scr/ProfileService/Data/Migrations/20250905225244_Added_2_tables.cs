using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProfileService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Added_2_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "ProfileSettings");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "ProfileSettings");

            migrationBuilder.AddColumn<int>(
                name: "LanguageId",
                table: "ProfileSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeZoneId",
                table: "ProfileSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Timezones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UtcOffset = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timezones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSettings_LanguageId",
                table: "ProfileSettings",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSettings_TimeZoneId",
                table: "ProfileSettings",
                column: "TimeZoneId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileSettings_Languages_LanguageId",
                table: "ProfileSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileSettings_Timezones_TimeZoneId",
                table: "ProfileSettings");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Timezones");

            migrationBuilder.DropIndex(
                name: "IX_ProfileSettings_LanguageId",
                table: "ProfileSettings");

            migrationBuilder.DropIndex(
                name: "IX_ProfileSettings_TimeZoneId",
                table: "ProfileSettings");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "ProfileSettings");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "ProfileSettings");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "ProfileSettings",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "ProfileSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
