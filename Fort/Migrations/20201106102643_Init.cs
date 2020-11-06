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
                    StCityId = table.Column<Guid>(nullable: false),
                    NdCityId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roads", x => new { x.StCityId, x.NdCityId });
                    table.ForeignKey(
                        name: "FK_Roads_Cities_NdCityId",
                        column: x => x.NdCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Roads_Cities_StCityId",
                        column: x => x.StCityId,
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
                    StIsSource = table.Column<bool>(nullable: false),
                    StCityId = table.Column<Guid>(nullable: false),
                    NdCityId = table.Column<Guid>(nullable: false),
                    TurnId = table.Column<int>(nullable: false),
                    Amount = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => new { x.StCityId, x.NdCityId, x.TurnId, x.StIsSource });
                    table.ForeignKey(
                        name: "FK_Orders_Cities_NdCityId",
                        column: x => x.NdCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Cities_StCityId",
                        column: x => x.StCityId,
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
                        name: "FK_Orders_CityOccupations_NdCityId_TurnId",
                        columns: x => new { x.NdCityId, x.TurnId },
                        principalTable: "CityOccupations",
                        principalColumns: new[] { "CityId", "TurnId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Roads_StCityId_NdCityId",
                        columns: x => new { x.StCityId, x.NdCityId },
                        principalTable: "Roads",
                        principalColumns: new[] { "StCityId", "NdCityId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_CityOccupations_StCityId_TurnId",
                        columns: x => new { x.StCityId, x.TurnId },
                        principalTable: "CityOccupations",
                        principalColumns: new[] { "CityId", "TurnId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "DefenceCoef", "PopulationGrowCoef", "X", "Y" },
                values: new object[,]
                {
                    { new Guid("fd6aec80-dca3-4d64-b53c-18b7a93eb550"), 1.0, 1.0, 714, 124 },
                    { new Guid("a591ed84-9567-4d05-9a92-95e1b233b36c"), 1.0, 1.0, 150, 111 },
                    { new Guid("29e29893-dc9a-4958-b82b-f64ef83f9f7e"), 1.0, 1.0, 220, 182 },
                    { new Guid("353b3a8d-4598-45f5-ad5c-3bec55918665"), 1.0, 1.0, 372, 236 },
                    { new Guid("23aad6a1-0e21-4d93-a7e0-60d099793736"), 1.0, 1.0, 235, 299 },
                    { new Guid("487f8d99-99a3-4c58-90e1-e75615cc080c"), 1.0, 1.0, 193, 401 },
                    { new Guid("8db03f1c-ffd5-4895-8a64-cbdfd52c6aa1"), 1.0, 1.0, 328, 415 },
                    { new Guid("675a39ff-499d-4735-b54a-e7f8502edb33"), 1.0, 1.0, 428, 462 },
                    { new Guid("5d6a5e74-61cd-4f45-941d-e5bb8a81d66e"), 1.0, 1.0, 356, 548 },
                    { new Guid("965997a3-604a-46ea-a19a-ad8a9b3bc37e"), 1.0, 1.0, 418, 794 },
                    { new Guid("1b584abf-2d44-44fa-af36-2aaa97405e87"), 1.0, 1.0, 360, 680 },
                    { new Guid("7ef620a5-9462-452a-b779-ca343f470625"), 1.0, 1.0, 1616, 745 },
                    { new Guid("56f636ea-4898-4a14-be55-295f21fb663a"), 1.0, 1.0, 1753, 845 },
                    { new Guid("ec88c63d-214b-4980-9a0a-55d8d1505b2c"), 1.0, 1.0, 1554, 313 },
                    { new Guid("95037d18-01e5-483f-93ec-712f87298a76"), 1.0, 1.0, 889, 665 },
                    { new Guid("43398d9e-69c5-4203-a66b-5e8d67b82c90"), 1.0, 1.0, 506, 340 },
                    { new Guid("494df006-1afd-4b96-bcf3-9f0c4442965d"), 1.0, 1.0, 1079, 213 },
                    { new Guid("c68d7841-84c3-454a-9e2b-4c3d8e85e8da"), 1.0, 1.0, 335, 321 },
                    { new Guid("a7861024-9eb4-4536-a0a9-e063022ae673"), 1.0, 1.0, 660, 800 },
                    { new Guid("74d20d91-a837-4f24-a9dd-4763e0caac4b"), 1.0, 1.0, 708, 348 },
                    { new Guid("208c67f5-ad51-490f-9f47-6b555ebbfc3a"), 1.0, 1.0, 1214, 671 },
                    { new Guid("cd9afdb5-3c6b-423e-9247-a8d26ba0a0a3"), 1.0, 1.0, 611, 962 },
                    { new Guid("b890df70-d9b3-41f6-a624-be238c563e29"), 1.0, 1.0, 967, 213 },
                    { new Guid("6e774576-1d08-43b8-beb8-e278f21679a1"), 1.0, 1.0, 473, 543 },
                    { new Guid("cd3c9dc6-4ecf-4e07-bfbd-607a074edb5d"), 1.0, 1.0, 286, 119 },
                    { new Guid("6bdf365d-d8c9-42f3-9fd6-8487d5704759"), 1.0, 1.0, 1106, 423 },
                    { new Guid("6e591c23-5b60-4f81-ae3e-1c57e6c63962"), 1.0, 1.0, 1211, 556 },
                    { new Guid("aafbf738-869b-4f13-81a9-55847b242d7a"), 1.0, 1.0, 870, 223 },
                    { new Guid("318137aa-fcf8-430a-b7b6-c98e2215510c"), 1.0, 1.0, 727, 438 },
                    { new Guid("da6b69f7-d94f-42ca-bad1-54f1906102c6"), 1.0, 1.0, 1067, 685 },
                    { new Guid("e315725c-fc53-4be6-9da1-1e259ab37748"), 1.0, 1.0, 1765, 518 },
                    { new Guid("7aa73bb9-a0d3-4ce3-a1b4-21523befd582"), 1.0, 1.0, 144, 310 },
                    { new Guid("87ae0df7-c6b5-4399-8386-f821b0340727"), 1.0, 1.0, 1757, 700 },
                    { new Guid("f49609e4-10c0-4aa8-81dd-952d6a4319dd"), 1.0, 1.0, 480, 190 },
                    { new Guid("b253f54e-5458-45eb-b294-f6e4bde9053a"), 1.0, 1.0, 456, 79 },
                    { new Guid("36a87ae2-ab57-4b9e-965d-42e7dc4cf0f1"), 1.0, 1.0, 1238, 134 },
                    { new Guid("a5c86f14-024e-4e31-8b8a-8218e8d9daa9"), 1.0, 1.0, 431, 931 },
                    { new Guid("c8784857-2efe-4fad-b278-178b2b63e8db"), 1.0, 1.0, 544, 720 },
                    { new Guid("5039ec78-3834-4922-97d8-18785aa5b187"), 1.0, 1.0, 439, 646 },
                    { new Guid("95f92740-ee83-4054-b957-b01be3879d85"), 1.0, 1.0, 562, 624 },
                    { new Guid("49ef30e1-000d-4b64-a13d-3d1828be6781"), 1.0, 1.0, 698, 636 },
                    { new Guid("5acf8b05-b2ad-4e00-87a8-541d977717da"), 1.0, 1.0, 899, 798 },
                    { new Guid("7a0b25d1-2edd-49cd-a37c-ae5b6550aa89"), 1.0, 1.0, 1460, 574 },
                    { new Guid("c517921b-d088-45cd-a456-2c613a4feb9e"), 1.0, 1.0, 1624, 615 },
                    { new Guid("a75127b3-d2a7-4d8b-9f1d-986297ecece1"), 1.0, 1.0, 1476, 769 },
                    { new Guid("95e8d61e-068a-4374-9883-89224161c0d9"), 1.0, 1.0, 1441, 384 },
                    { new Guid("178b0049-509f-42da-9008-d49e5ea98142"), 1.0, 1.0, 1264, 415 },
                    { new Guid("20ae2fa2-907b-4f57-9d24-76dbc53972ff"), 1.0, 1.0, 1354, 313 },
                    { new Guid("f39e5b14-6633-4b17-b86d-b06076d0430c"), 1.0, 1.0, 1182, 228 },
                    { new Guid("8cbb321c-901e-4ccf-8981-88967a12481e"), 1.0, 1.0, 593, 110 },
                    { new Guid("d285bf5a-5439-4cb7-82fb-d6f59ee09af5"), 1.0, 1.0, 1572, 878 },
                    { new Guid("667d6946-47cf-4db6-bbc1-3c911bcc43f6"), 1.0, 1.0, 1445, 121 },
                    { new Guid("e5f97401-4aff-4527-94af-c517f152156c"), 1.0, 1.0, 1154, 111 },
                    { new Guid("161e17f0-c63d-4793-b8c1-76d5d863518c"), 1.0, 1.0, 1001, 143 },
                    { new Guid("6e660a3f-7c0c-4597-91ae-2c877518da0a"), 1.0, 1.0, 1003, 315 },
                    { new Guid("9d4a6fc5-a5ea-4e07-8354-15dff81c356d"), 1.0, 1.0, 962, 464 },
                    { new Guid("4d814ee8-8d6a-49e2-93cb-71199f987f74"), 1.0, 1.0, 1019, 518 },
                    { new Guid("5fad7a28-61c8-46d3-916f-ead505a06e64"), 1.0, 1.0, 932, 586 },
                    { new Guid("1997df31-8e36-4166-91ba-423bcef19794"), 1.0, 1.0, 829, 507 },
                    { new Guid("b5a67442-19d8-471e-9c32-05a558db9fc7"), 1.0, 1.0, 905, 387 },
                    { new Guid("08c12c35-5306-49d3-bcdd-609f9213265a"), 1.0, 1.0, 805, 364 },
                    { new Guid("44e5bdfa-329a-41c4-aee2-a2ffc07211b7"), 1.0, 1.0, 1106, 299 },
                    { new Guid("27d4e3fc-e42e-4187-82d7-4f30e6ddb3e8"), 1.0, 1.0, 836, 143 },
                    { new Guid("b41531cb-c23e-4a7c-a149-ea86d942bfb2"), 1.0, 1.0, 784, 205 },
                    { new Guid("2431da6c-ddf9-4653-8344-d66de031955e"), 1.0, 1.0, 769, 286 },
                    { new Guid("ac8bf4f3-8842-4d6a-8b69-524fd944e19a"), 1.0, 1.0, 631, 307 },
                    { new Guid("93205cf8-ec43-4464-b717-685442dcd641"), 1.0, 1.0, 1345, 182 },
                    { new Guid("834d65e5-7da3-4a42-871e-80f58d19b716"), 1.0, 1.0, 646, 467 }
                });

            migrationBuilder.InsertData(
                table: "Teams",
                columns: new[] { "Id", "ArmyStrengthCoef", "Color", "ColorLight", "PopulationGrowthCoef" },
                values: new object[,]
                {
                    { new Guid("3541fb45-b40f-4199-a860-2f731c3341a8"), 1.0, "#4b7183", "#98b6c4", 1.0 },
                    { new Guid("4b44b52e-d354-4600-bb4a-ba87db0c6bc5"), 1.0, "#52834b", "#9dc498", 1.0 },
                    { new Guid("2ebcc534-9b62-448b-b8cb-7e74f7d9c8b6"), 1.0, "#83824b", "#c4c498", 1.0 }
                });

            migrationBuilder.InsertData(
                table: "Roads",
                columns: new[] { "StCityId", "NdCityId" },
                values: new object[,]
                {
                    { new Guid("a5c86f14-024e-4e31-8b8a-8218e8d9daa9"), new Guid("c8784857-2efe-4fad-b278-178b2b63e8db") },
                    { new Guid("7aa73bb9-a0d3-4ce3-a1b4-21523befd582"), new Guid("a591ed84-9567-4d05-9a92-95e1b233b36c") },
                    { new Guid("29e29893-dc9a-4958-b82b-f64ef83f9f7e"), new Guid("7aa73bb9-a0d3-4ce3-a1b4-21523befd582") },
                    { new Guid("23aad6a1-0e21-4d93-a7e0-60d099793736"), new Guid("7aa73bb9-a0d3-4ce3-a1b4-21523befd582") },
                    { new Guid("487f8d99-99a3-4c58-90e1-e75615cc080c"), new Guid("7aa73bb9-a0d3-4ce3-a1b4-21523befd582") },
                    { new Guid("43398d9e-69c5-4203-a66b-5e8d67b82c90"), new Guid("c68d7841-84c3-454a-9e2b-4c3d8e85e8da") },
                    { new Guid("29e29893-dc9a-4958-b82b-f64ef83f9f7e"), new Guid("c68d7841-84c3-454a-9e2b-4c3d8e85e8da") },
                    { new Guid("8db03f1c-ffd5-4895-8a64-cbdfd52c6aa1"), new Guid("c68d7841-84c3-454a-9e2b-4c3d8e85e8da") },
                    { new Guid("23aad6a1-0e21-4d93-a7e0-60d099793736"), new Guid("c68d7841-84c3-454a-9e2b-4c3d8e85e8da") },
                    { new Guid("494df006-1afd-4b96-bcf3-9f0c4442965d"), new Guid("e5f97401-4aff-4527-94af-c517f152156c") },
                    { new Guid("161e17f0-c63d-4793-b8c1-76d5d863518c"), new Guid("494df006-1afd-4b96-bcf3-9f0c4442965d") },
                    { new Guid("494df006-1afd-4b96-bcf3-9f0c4442965d"), new Guid("f39e5b14-6633-4b17-b86d-b06076d0430c") },
                    { new Guid("494df006-1afd-4b96-bcf3-9f0c4442965d"), new Guid("6e660a3f-7c0c-4597-91ae-2c877518da0a") },
                    { new Guid("43398d9e-69c5-4203-a66b-5e8d67b82c90"), new Guid("ac8bf4f3-8842-4d6a-8b69-524fd944e19a") },
                    { new Guid("43398d9e-69c5-4203-a66b-5e8d67b82c90"), new Guid("834d65e5-7da3-4a42-871e-80f58d19b716") },
                    { new Guid("e315725c-fc53-4be6-9da1-1e259ab37748"), new Guid("ec88c63d-214b-4980-9a0a-55d8d1505b2c") },
                    { new Guid("43398d9e-69c5-4203-a66b-5e8d67b82c90"), new Guid("675a39ff-499d-4735-b54a-e7f8502edb33") },
                    { new Guid("5fad7a28-61c8-46d3-916f-ead505a06e64"), new Guid("95037d18-01e5-483f-93ec-712f87298a76") },
                    { new Guid("1997df31-8e36-4166-91ba-423bcef19794"), new Guid("95037d18-01e5-483f-93ec-712f87298a76") },
                    { new Guid("5acf8b05-b2ad-4e00-87a8-541d977717da"), new Guid("95037d18-01e5-483f-93ec-712f87298a76") },
                    { new Guid("667d6946-47cf-4db6-bbc1-3c911bcc43f6"), new Guid("ec88c63d-214b-4980-9a0a-55d8d1505b2c") },
                    { new Guid("93205cf8-ec43-4464-b717-685442dcd641"), new Guid("ec88c63d-214b-4980-9a0a-55d8d1505b2c") },
                    { new Guid("20ae2fa2-907b-4f57-9d24-76dbc53972ff"), new Guid("ec88c63d-214b-4980-9a0a-55d8d1505b2c") },
                    { new Guid("95e8d61e-068a-4374-9883-89224161c0d9"), new Guid("ec88c63d-214b-4980-9a0a-55d8d1505b2c") },
                    { new Guid("56f636ea-4898-4a14-be55-295f21fb663a"), new Guid("7ef620a5-9462-452a-b779-ca343f470625") },
                    { new Guid("56f636ea-4898-4a14-be55-295f21fb663a"), new Guid("d285bf5a-5439-4cb7-82fb-d6f59ee09af5") },
                    { new Guid("56f636ea-4898-4a14-be55-295f21fb663a"), new Guid("a75127b3-d2a7-4d8b-9f1d-986297ecece1") },
                    { new Guid("7ef620a5-9462-452a-b779-ca343f470625"), new Guid("a75127b3-d2a7-4d8b-9f1d-986297ecece1") },
                    { new Guid("7ef620a5-9462-452a-b779-ca343f470625"), new Guid("c517921b-d088-45cd-a456-2c613a4feb9e") },
                    { new Guid("7a0b25d1-2edd-49cd-a37c-ae5b6550aa89"), new Guid("7ef620a5-9462-452a-b779-ca343f470625") },
                    { new Guid("1b584abf-2d44-44fa-af36-2aaa97405e87"), new Guid("5d6a5e74-61cd-4f45-941d-e5bb8a81d66e") },
                    { new Guid("353b3a8d-4598-45f5-ad5c-3bec55918665"), new Guid("43398d9e-69c5-4203-a66b-5e8d67b82c90") },
                    { new Guid("1b584abf-2d44-44fa-af36-2aaa97405e87"), new Guid("5039ec78-3834-4922-97d8-18785aa5b187") },
                    { new Guid("c517921b-d088-45cd-a456-2c613a4feb9e"), new Guid("e315725c-fc53-4be6-9da1-1e259ab37748") },
                    { new Guid("5acf8b05-b2ad-4e00-87a8-541d977717da"), new Guid("da6b69f7-d94f-42ca-bad1-54f1906102c6") },
                    { new Guid("49ef30e1-000d-4b64-a13d-3d1828be6781"), new Guid("a7861024-9eb4-4536-a0a9-e063022ae673") },
                    { new Guid("74d20d91-a837-4f24-a9dd-4763e0caac4b"), new Guid("ac8bf4f3-8842-4d6a-8b69-524fd944e19a") },
                    { new Guid("208c67f5-ad51-490f-9f47-6b555ebbfc3a"), new Guid("6e591c23-5b60-4f81-ae3e-1c57e6c63962") },
                    { new Guid("5acf8b05-b2ad-4e00-87a8-541d977717da"), new Guid("cd9afdb5-3c6b-423e-9247-a8d26ba0a0a3") },
                    { new Guid("a5c86f14-024e-4e31-8b8a-8218e8d9daa9"), new Guid("cd9afdb5-3c6b-423e-9247-a8d26ba0a0a3") },
                    { new Guid("161e17f0-c63d-4793-b8c1-76d5d863518c"), new Guid("b890df70-d9b3-41f6-a624-be238c563e29") },
                    { new Guid("6e774576-1d08-43b8-beb8-e278f21679a1"), new Guid("834d65e5-7da3-4a42-871e-80f58d19b716") },
                    { new Guid("5d6a5e74-61cd-4f45-941d-e5bb8a81d66e"), new Guid("6e774576-1d08-43b8-beb8-e278f21679a1") },
                    { new Guid("675a39ff-499d-4735-b54a-e7f8502edb33"), new Guid("6e774576-1d08-43b8-beb8-e278f21679a1") },
                    { new Guid("44e5bdfa-329a-41c4-aee2-a2ffc07211b7"), new Guid("6bdf365d-d8c9-42f3-9fd6-8487d5704759") },
                    { new Guid("56f636ea-4898-4a14-be55-295f21fb663a"), new Guid("87ae0df7-c6b5-4399-8386-f821b0340727") },
                    { new Guid("87ae0df7-c6b5-4399-8386-f821b0340727"), new Guid("e315725c-fc53-4be6-9da1-1e259ab37748") },
                    { new Guid("7ef620a5-9462-452a-b779-ca343f470625"), new Guid("87ae0df7-c6b5-4399-8386-f821b0340727") },
                    { new Guid("87ae0df7-c6b5-4399-8386-f821b0340727"), new Guid("c517921b-d088-45cd-a456-2c613a4feb9e") },
                    { new Guid("a75127b3-d2a7-4d8b-9f1d-986297ecece1"), new Guid("da6b69f7-d94f-42ca-bad1-54f1906102c6") },
                    { new Guid("6e591c23-5b60-4f81-ae3e-1c57e6c63962"), new Guid("7a0b25d1-2edd-49cd-a37c-ae5b6550aa89") },
                    { new Guid("6e591c23-5b60-4f81-ae3e-1c57e6c63962"), new Guid("da6b69f7-d94f-42ca-bad1-54f1906102c6") },
                    { new Guid("4d814ee8-8d6a-49e2-93cb-71199f987f74"), new Guid("6e591c23-5b60-4f81-ae3e-1c57e6c63962") },
                    { new Guid("178b0049-509f-42da-9008-d49e5ea98142"), new Guid("6e591c23-5b60-4f81-ae3e-1c57e6c63962") },
                    { new Guid("2431da6c-ddf9-4653-8344-d66de031955e"), new Guid("aafbf738-869b-4f13-81a9-55847b242d7a") },
                    { new Guid("aafbf738-869b-4f13-81a9-55847b242d7a"), new Guid("b5a67442-19d8-471e-9c32-05a558db9fc7") },
                    { new Guid("08c12c35-5306-49d3-bcdd-609f9213265a"), new Guid("aafbf738-869b-4f13-81a9-55847b242d7a") },
                    { new Guid("6e660a3f-7c0c-4597-91ae-2c877518da0a"), new Guid("aafbf738-869b-4f13-81a9-55847b242d7a") },
                    { new Guid("27d4e3fc-e42e-4187-82d7-4f30e6ddb3e8"), new Guid("aafbf738-869b-4f13-81a9-55847b242d7a") },
                    { new Guid("1997df31-8e36-4166-91ba-423bcef19794"), new Guid("318137aa-fcf8-430a-b7b6-c98e2215510c") },
                    { new Guid("08c12c35-5306-49d3-bcdd-609f9213265a"), new Guid("318137aa-fcf8-430a-b7b6-c98e2215510c") },
                    { new Guid("318137aa-fcf8-430a-b7b6-c98e2215510c"), new Guid("834d65e5-7da3-4a42-871e-80f58d19b716") },
                    { new Guid("318137aa-fcf8-430a-b7b6-c98e2215510c"), new Guid("95f92740-ee83-4054-b957-b01be3879d85") },
                    { new Guid("5fad7a28-61c8-46d3-916f-ead505a06e64"), new Guid("da6b69f7-d94f-42ca-bad1-54f1906102c6") },
                    { new Guid("95037d18-01e5-483f-93ec-712f87298a76"), new Guid("da6b69f7-d94f-42ca-bad1-54f1906102c6") },
                    { new Guid("6e591c23-5b60-4f81-ae3e-1c57e6c63962"), new Guid("a75127b3-d2a7-4d8b-9f1d-986297ecece1") },
                    { new Guid("1b584abf-2d44-44fa-af36-2aaa97405e87"), new Guid("965997a3-604a-46ea-a19a-ad8a9b3bc37e") },
                    { new Guid("965997a3-604a-46ea-a19a-ad8a9b3bc37e"), new Guid("c8784857-2efe-4fad-b278-178b2b63e8db") },
                    { new Guid("08c12c35-5306-49d3-bcdd-609f9213265a"), new Guid("b5a67442-19d8-471e-9c32-05a558db9fc7") },
                    { new Guid("1997df31-8e36-4166-91ba-423bcef19794"), new Guid("b5a67442-19d8-471e-9c32-05a558db9fc7") },
                    { new Guid("9d4a6fc5-a5ea-4e07-8354-15dff81c356d"), new Guid("b5a67442-19d8-471e-9c32-05a558db9fc7") },
                    { new Guid("6e660a3f-7c0c-4597-91ae-2c877518da0a"), new Guid("b5a67442-19d8-471e-9c32-05a558db9fc7") },
                    { new Guid("1997df31-8e36-4166-91ba-423bcef19794"), new Guid("49ef30e1-000d-4b64-a13d-3d1828be6781") },
                    { new Guid("1997df31-8e36-4166-91ba-423bcef19794"), new Guid("5fad7a28-61c8-46d3-916f-ead505a06e64") },
                    { new Guid("1997df31-8e36-4166-91ba-423bcef19794"), new Guid("9d4a6fc5-a5ea-4e07-8354-15dff81c356d") },
                    { new Guid("4d814ee8-8d6a-49e2-93cb-71199f987f74"), new Guid("5fad7a28-61c8-46d3-916f-ead505a06e64") },
                    { new Guid("4d814ee8-8d6a-49e2-93cb-71199f987f74"), new Guid("9d4a6fc5-a5ea-4e07-8354-15dff81c356d") },
                    { new Guid("178b0049-509f-42da-9008-d49e5ea98142"), new Guid("4d814ee8-8d6a-49e2-93cb-71199f987f74") },
                    { new Guid("161e17f0-c63d-4793-b8c1-76d5d863518c"), new Guid("e5f97401-4aff-4527-94af-c517f152156c") },
                    { new Guid("e5f97401-4aff-4527-94af-c517f152156c"), new Guid("f39e5b14-6633-4b17-b86d-b06076d0430c") },
                    { new Guid("36a87ae2-ab57-4b9e-965d-42e7dc4cf0f1"), new Guid("e5f97401-4aff-4527-94af-c517f152156c") },
                    { new Guid("20ae2fa2-907b-4f57-9d24-76dbc53972ff"), new Guid("93205cf8-ec43-4464-b717-685442dcd641") },
                    { new Guid("93205cf8-ec43-4464-b717-685442dcd641"), new Guid("f39e5b14-6633-4b17-b86d-b06076d0430c") },
                    { new Guid("5039ec78-3834-4922-97d8-18785aa5b187"), new Guid("965997a3-604a-46ea-a19a-ad8a9b3bc37e") },
                    { new Guid("36a87ae2-ab57-4b9e-965d-42e7dc4cf0f1"), new Guid("93205cf8-ec43-4464-b717-685442dcd641") },
                    { new Guid("36a87ae2-ab57-4b9e-965d-42e7dc4cf0f1"), new Guid("667d6946-47cf-4db6-bbc1-3c911bcc43f6") },
                    { new Guid("36a87ae2-ab57-4b9e-965d-42e7dc4cf0f1"), new Guid("f39e5b14-6633-4b17-b86d-b06076d0430c") },
                    { new Guid("20ae2fa2-907b-4f57-9d24-76dbc53972ff"), new Guid("f39e5b14-6633-4b17-b86d-b06076d0430c") },
                    { new Guid("20ae2fa2-907b-4f57-9d24-76dbc53972ff"), new Guid("95e8d61e-068a-4374-9883-89224161c0d9") },
                    { new Guid("178b0049-509f-42da-9008-d49e5ea98142"), new Guid("20ae2fa2-907b-4f57-9d24-76dbc53972ff") },
                    { new Guid("178b0049-509f-42da-9008-d49e5ea98142"), new Guid("7a0b25d1-2edd-49cd-a37c-ae5b6550aa89") },
                    { new Guid("7a0b25d1-2edd-49cd-a37c-ae5b6550aa89"), new Guid("95e8d61e-068a-4374-9883-89224161c0d9") },
                    { new Guid("7a0b25d1-2edd-49cd-a37c-ae5b6550aa89"), new Guid("a75127b3-d2a7-4d8b-9f1d-986297ecece1") },
                    { new Guid("a75127b3-d2a7-4d8b-9f1d-986297ecece1"), new Guid("d285bf5a-5439-4cb7-82fb-d6f59ee09af5") },
                    { new Guid("7a0b25d1-2edd-49cd-a37c-ae5b6550aa89"), new Guid("c517921b-d088-45cd-a456-2c613a4feb9e") },
                    { new Guid("49ef30e1-000d-4b64-a13d-3d1828be6781"), new Guid("5acf8b05-b2ad-4e00-87a8-541d977717da") },
                    { new Guid("49ef30e1-000d-4b64-a13d-3d1828be6781"), new Guid("95f92740-ee83-4054-b957-b01be3879d85") },
                    { new Guid("834d65e5-7da3-4a42-871e-80f58d19b716"), new Guid("95f92740-ee83-4054-b957-b01be3879d85") },
                    { new Guid("5039ec78-3834-4922-97d8-18785aa5b187"), new Guid("95f92740-ee83-4054-b957-b01be3879d85") },
                    { new Guid("667d6946-47cf-4db6-bbc1-3c911bcc43f6"), new Guid("93205cf8-ec43-4464-b717-685442dcd641") },
                    { new Guid("95f92740-ee83-4054-b957-b01be3879d85"), new Guid("c8784857-2efe-4fad-b278-178b2b63e8db") },
                    { new Guid("178b0049-509f-42da-9008-d49e5ea98142"), new Guid("44e5bdfa-329a-41c4-aee2-a2ffc07211b7") },
                    { new Guid("44e5bdfa-329a-41c4-aee2-a2ffc07211b7"), new Guid("9d4a6fc5-a5ea-4e07-8354-15dff81c356d") },
                    { new Guid("965997a3-604a-46ea-a19a-ad8a9b3bc37e"), new Guid("a5c86f14-024e-4e31-8b8a-8218e8d9daa9") },
                    { new Guid("5d6a5e74-61cd-4f45-941d-e5bb8a81d66e"), new Guid("8db03f1c-ffd5-4895-8a64-cbdfd52c6aa1") },
                    { new Guid("5d6a5e74-61cd-4f45-941d-e5bb8a81d66e"), new Guid("675a39ff-499d-4735-b54a-e7f8502edb33") },
                    { new Guid("5039ec78-3834-4922-97d8-18785aa5b187"), new Guid("5d6a5e74-61cd-4f45-941d-e5bb8a81d66e") },
                    { new Guid("675a39ff-499d-4735-b54a-e7f8502edb33"), new Guid("8db03f1c-ffd5-4895-8a64-cbdfd52c6aa1") },
                    { new Guid("675a39ff-499d-4735-b54a-e7f8502edb33"), new Guid("834d65e5-7da3-4a42-871e-80f58d19b716") },
                    { new Guid("487f8d99-99a3-4c58-90e1-e75615cc080c"), new Guid("8db03f1c-ffd5-4895-8a64-cbdfd52c6aa1") },
                    { new Guid("23aad6a1-0e21-4d93-a7e0-60d099793736"), new Guid("29e29893-dc9a-4958-b82b-f64ef83f9f7e") },
                    { new Guid("353b3a8d-4598-45f5-ad5c-3bec55918665"), new Guid("f49609e4-10c0-4aa8-81dd-952d6a4319dd") },
                    { new Guid("353b3a8d-4598-45f5-ad5c-3bec55918665"), new Guid("cd3c9dc6-4ecf-4e07-bfbd-607a074edb5d") },
                    { new Guid("29e29893-dc9a-4958-b82b-f64ef83f9f7e"), new Guid("353b3a8d-4598-45f5-ad5c-3bec55918665") },
                    { new Guid("29e29893-dc9a-4958-b82b-f64ef83f9f7e"), new Guid("cd3c9dc6-4ecf-4e07-bfbd-607a074edb5d") },
                    { new Guid("29e29893-dc9a-4958-b82b-f64ef83f9f7e"), new Guid("a591ed84-9567-4d05-9a92-95e1b233b36c") },
                    { new Guid("a591ed84-9567-4d05-9a92-95e1b233b36c"), new Guid("cd3c9dc6-4ecf-4e07-bfbd-607a074edb5d") },
                    { new Guid("44e5bdfa-329a-41c4-aee2-a2ffc07211b7"), new Guid("f39e5b14-6633-4b17-b86d-b06076d0430c") },
                    { new Guid("b253f54e-5458-45eb-b294-f6e4bde9053a"), new Guid("cd3c9dc6-4ecf-4e07-bfbd-607a074edb5d") },
                    { new Guid("b253f54e-5458-45eb-b294-f6e4bde9053a"), new Guid("f49609e4-10c0-4aa8-81dd-952d6a4319dd") },
                    { new Guid("b41531cb-c23e-4a7c-a149-ea86d942bfb2"), new Guid("f49609e4-10c0-4aa8-81dd-952d6a4319dd") },
                    { new Guid("8cbb321c-901e-4ccf-8981-88967a12481e"), new Guid("f49609e4-10c0-4aa8-81dd-952d6a4319dd") },
                    { new Guid("ac8bf4f3-8842-4d6a-8b69-524fd944e19a"), new Guid("f49609e4-10c0-4aa8-81dd-952d6a4319dd") },
                    { new Guid("8cbb321c-901e-4ccf-8981-88967a12481e"), new Guid("b41531cb-c23e-4a7c-a149-ea86d942bfb2") },
                    { new Guid("8cbb321c-901e-4ccf-8981-88967a12481e"), new Guid("fd6aec80-dca3-4d64-b53c-18b7a93eb550") },
                    { new Guid("834d65e5-7da3-4a42-871e-80f58d19b716"), new Guid("ac8bf4f3-8842-4d6a-8b69-524fd944e19a") },
                    { new Guid("2431da6c-ddf9-4653-8344-d66de031955e"), new Guid("ac8bf4f3-8842-4d6a-8b69-524fd944e19a") },
                    { new Guid("2431da6c-ddf9-4653-8344-d66de031955e"), new Guid("b41531cb-c23e-4a7c-a149-ea86d942bfb2") },
                    { new Guid("08c12c35-5306-49d3-bcdd-609f9213265a"), new Guid("2431da6c-ddf9-4653-8344-d66de031955e") },
                    { new Guid("27d4e3fc-e42e-4187-82d7-4f30e6ddb3e8"), new Guid("b41531cb-c23e-4a7c-a149-ea86d942bfb2") },
                    { new Guid("b41531cb-c23e-4a7c-a149-ea86d942bfb2"), new Guid("fd6aec80-dca3-4d64-b53c-18b7a93eb550") },
                    { new Guid("161e17f0-c63d-4793-b8c1-76d5d863518c"), new Guid("27d4e3fc-e42e-4187-82d7-4f30e6ddb3e8") },
                    { new Guid("27d4e3fc-e42e-4187-82d7-4f30e6ddb3e8"), new Guid("fd6aec80-dca3-4d64-b53c-18b7a93eb550") },
                    { new Guid("8cbb321c-901e-4ccf-8981-88967a12481e"), new Guid("b253f54e-5458-45eb-b294-f6e4bde9053a") },
                    { new Guid("5039ec78-3834-4922-97d8-18785aa5b187"), new Guid("834d65e5-7da3-4a42-871e-80f58d19b716") }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "ImageUrl", "IsAdmin", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TeamId", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { new Guid("8c1d1996-4d41-4892-82d9-29d00b986958"), 0, "932a8c18-5e96-4db3-926e-f52142d6c5d6", "b", false, null, false, false, null, null, null, "AQAAAAEAACcQAAAAEJCNVHZeDNTSqJjqKFlp7CRZEBMDkQATuj7FKxvtpO2/q3w8xORbQzUF1bZ/FKr2cA==", null, false, null, new Guid("3541fb45-b40f-4199-a860-2f731c3341a8"), false, "B" },
                    { new Guid("08d7d312-5b73-86fb-7e7c-5d6b08a519b0"), 0, "f0d27d0f-078e-4edd-8732-10a8ae6cbf5a", "a", false, null, false, false, null, null, null, "AQAAAAEAACcQAAAAEFuzvZx1UOwIMC9i6XWUw2itbgoa2/Fg/tUSCH8NhJdL9MPSur/ZN/wgiEVBQnBmyw==", null, false, null, new Guid("4b44b52e-d354-4600-bb4a-ba87db0c6bc5"), false, "A" },
                    { new Guid("07af142f-2ba3-4499-b9a7-d3347920b04a"), 0, "bf523aed-8cd4-4793-a602-3bce8f334a95", "c", false, null, false, false, null, null, null, "AQAAAAEAACcQAAAAECxoze2Fh8lG09895QNaWlpHs5NX0RCQTWwPoLaj+FDp7xvpBbnIGs/ocZ53v7yJSg==", null, false, null, new Guid("2ebcc534-9b62-448b-b8cb-7e74f7d9c8b6"), false, "C" }
                });

            migrationBuilder.InsertData(
                table: "StartingPositions",
                columns: new[] { "CityId", "Army", "UserId" },
                values: new object[] { new Guid("a591ed84-9567-4d05-9a92-95e1b233b36c"), 15, new Guid("08d7d312-5b73-86fb-7e7c-5d6b08a519b0") });

            migrationBuilder.InsertData(
                table: "StartingPositions",
                columns: new[] { "CityId", "Army", "UserId" },
                values: new object[] { new Guid("2431da6c-ddf9-4653-8344-d66de031955e"), 15, new Guid("8c1d1996-4d41-4892-82d9-29d00b986958") });

            migrationBuilder.InsertData(
                table: "StartingPositions",
                columns: new[] { "CityId", "Army", "UserId" },
                values: new object[] { new Guid("aafbf738-869b-4f13-81a9-55847b242d7a"), 15, new Guid("07af142f-2ba3-4499-b9a7-d3347920b04a") });

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
                name: "IX_Orders_NdCityId_TurnId",
                table: "Orders",
                columns: new[] { "NdCityId", "TurnId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StCityId_TurnId",
                table: "Orders",
                columns: new[] { "StCityId", "TurnId" });

            migrationBuilder.CreateIndex(
                name: "IX_Roads_NdCityId",
                table: "Roads",
                column: "NdCityId");

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
                name: "CityOccupations");

            migrationBuilder.DropTable(
                name: "Roads");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
