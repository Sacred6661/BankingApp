using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProfileService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Countries_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "ProfileAddresses");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "ProfileAddresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Alpha2Code = table.Column<string>(type: "text", nullable: true),
                    Alpha3Code = table.Column<string>(type: "text", nullable: true),
                    NumericCode = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileAddresses_CountryId",
                table: "ProfileAddresses",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileAddresses_Countries_CountryId",
                table: "ProfileAddresses",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileAddresses_Countries_CountryId",
                table: "ProfileAddresses");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_ProfileAddresses_CountryId",
                table: "ProfileAddresses");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "ProfileAddresses");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "ProfileAddresses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
