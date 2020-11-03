using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fort.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    X = table.Column<int>(nullable: false),
                    Y = table.Column<int>(nullable: false),
                    PopulationGrowCoef = table.Column<double>(nullable: false),
                    DefenceCoef = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ArmyStrengthCoef = table.Column<double>(nullable: false),
                    PopulationGrowthCoef = table.Column<double>(nullable: false),
                    Color = table.Column<string>(maxLength: 10, nullable: false),
                    ColorLight = table.Column<string>(maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Turns",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    StartsAt = table.Column<DateTime>(nullable: true),
                    EndsAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roads",
                columns: table => new
                {
                    SourceId = table.Column<Guid>(nullable: false),
                    TargetId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roads", x => new { x.SourceId, x.TargetId });
                    table.ForeignKey(
                        name: "FK_Roads_Cities_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Roads_Cities_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    NormalizedUserName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    NormalizedEmail = table.Column<string>(nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    ImageUrl = table.Column<string>(maxLength: 100, nullable: true),
                    IsAdmin = table.Column<bool>(nullable: false),
                    TeamId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CityOccupations",
                columns: table => new
                {
                    CityId = table.Column<Guid>(nullable: false),
                    TurnId = table.Column<int>(nullable: false),
                    Army = table.Column<int>(nullable: false),
                    OwnerId = table.Column<Guid>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityOccupations", x => new { x.CityId, x.TurnId });
                    table.ForeignKey(
                        name: "FK_CityOccupations_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CityOccupations_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CityOccupations_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StartingPositions",
                columns: table => new
                {
                    CityId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Army = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartingPositions", x => x.CityId);
                    table.ForeignKey(
                        name: "FK_StartingPositions_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StartingPositions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    IsReverseDirection = table.Column<bool>(nullable: false),
                    SourceCityId = table.Column<Guid>(nullable: false),
                    TargetCityId = table.Column<Guid>(nullable: false),
                    TurnId = table.Column<int>(nullable: false),
                    Amount = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => new { x.SourceCityId, x.TargetCityId, x.TurnId, x.IsReverseDirection });
                    table.ForeignKey(
                        name: "FK_Orders_Cities_SourceCityId",
                        column: x => x.SourceCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Cities_TargetCityId",
                        column: x => x.TargetCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Roads_SourceCityId_TargetCityId",
                        columns: x => new { x.SourceCityId, x.TargetCityId },
                        principalTable: "Roads",
                        principalColumns: new[] { "SourceId", "TargetId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_CityOccupations_SourceCityId_TurnId",
                        columns: x => new { x.SourceCityId, x.TurnId },
                        principalTable: "CityOccupations",
                        principalColumns: new[] { "CityId", "TurnId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_CityOccupations_TargetCityId_TurnId",
                        columns: x => new { x.TargetCityId, x.TurnId },
                        principalTable: "CityOccupations",
                        principalColumns: new[] { "CityId", "TurnId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CityOccupations_OwnerId",
                table: "CityOccupations",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CityOccupations_TurnId",
                table: "CityOccupations",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TurnId",
                table: "Orders",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SourceCityId_TurnId",
                table: "Orders",
                columns: new[] { "SourceCityId", "TurnId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TargetCityId_TurnId",
                table: "Orders",
                columns: new[] { "TargetCityId", "TurnId" });

            migrationBuilder.CreateIndex(
                name: "IX_Roads_TargetId",
                table: "Roads",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_StartingPositions_UserId",
                table: "StartingPositions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TeamId",
                table: "Users",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "StartingPositions");

            migrationBuilder.DropTable(
                name: "Roads");

            migrationBuilder.DropTable(
                name: "CityOccupations");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
