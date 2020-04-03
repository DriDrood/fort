using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fort.Migrations
{
    public partial class NeutralCityOccupation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CityOccupations_Users_OwnerId",
                table: "CityOccupations");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "CityOccupations",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_CityOccupations_Users_OwnerId",
                table: "CityOccupations",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CityOccupations_Users_OwnerId",
                table: "CityOccupations");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "CityOccupations",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CityOccupations_Users_OwnerId",
                table: "CityOccupations",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
