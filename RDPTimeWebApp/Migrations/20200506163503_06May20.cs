using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace RDPTimeWebApp.Migrations
{
    public partial class Upd06May20 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "SCUD_Id",
                table: "Users",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VectorTime",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Month = table.Column<byte>(nullable: false),
                    Time = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VectorTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VectorTime_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VectorTime_UserId",
                table: "VectorTime",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VectorTime_Year_Month",
                table: "VectorTime",
                columns: new[] { "Year", "Month" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VectorTime");

            migrationBuilder.DropColumn(
                name: "SCUD_Id",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
