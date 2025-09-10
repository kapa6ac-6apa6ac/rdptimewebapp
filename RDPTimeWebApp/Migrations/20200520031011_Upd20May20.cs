using Microsoft.EntityFrameworkCore.Migrations;

namespace RDPTimeWebApp.Migrations
{
    public partial class Upd20May20 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "SCUD_Id",
            //    table: "Users");

            //migrationBuilder.AddColumn<int>(
            //    name: "ScudSlvId",
            //    table: "Users",
            //    nullable: true);

            migrationBuilder.RenameColumn(
                name: "SCUD_Id",
                table: "Users",
                newName: "ScudSlvId");

            migrationBuilder.AddColumn<int>(
                name: "ScudUfaId",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "ScudSlvId",
            //    table: "Users");

            migrationBuilder.DropColumn(
                name: "ScudUfaId",
                table: "Users");

            //migrationBuilder.AddColumn<int>(
            //    name: "SCUD_Id",
            //    table: "Users",
            //    type: "integer",
            //    nullable: true);

            migrationBuilder.RenameColumn(
                name: "ScudSlvId",
                table: "Users",
                newName: "SCUD_Id");
        }
    }
}
