using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Infastructure.Migrations
{
    /// <inheritdoc />
    public partial class EntityConfigurationsToModifyDesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Players_PlayerId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentRegistrations_Players_PlayerId",
                table: "TournamentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Rankings_PlayerId",
                table: "Rankings");

            migrationBuilder.DropIndex(
                name: "IX_PrizePools_TournamentId",
                table: "PrizePools");

            migrationBuilder.DropColumn(
                name: "CheckedIn",
                table: "TournamentRegistrations");

            migrationBuilder.DropColumn(
                name: "RegisteredDate",
                table: "TournamentRegistrations");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Rankings");

            migrationBuilder.DropColumn(
                name: "FirstPlacePrize",
                table: "PrizePools");

            migrationBuilder.DropColumn(
                name: "SecondPlacePrize",
                table: "PrizePools");

            migrationBuilder.DropColumn(
                name: "ThirdPlacePrize",
                table: "PrizePools");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tournaments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "Format",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Tournaments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Legs",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PointsForDraw",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PointsForLoss",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PointsForWin",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDeadline",
                table: "Tournaments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationOpenDate",
                table: "Tournaments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Tournaments",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAt",
                table: "TournamentRegistrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisqualificationReason",
                table: "TournamentRegistrations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisqualifiedAt",
                table: "TournamentRegistrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TournamentRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "GameId",
                table: "Rankings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PrizePools",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Method",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProvider",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderReference",
                table: "Payments",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TournamentId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Notifications",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PlayerTwoId",
                table: "Matches",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlayerOneId",
                table: "Matches",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "MatchWeek",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAt",
                table: "Matches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Games",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Games",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AuditLogs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Changes",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AuditLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrizeDistributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrizePoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrizeDistributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrizeDistributions_PrizePools_PrizePoolId",
                        column: x => x.PrizePoolId,
                        principalTable: "PrizePools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentStanding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Played = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Draws = table.Column<int>(type: "int", nullable: false),
                    Losses = table.Column<int>(type: "int", nullable: false),
                    ScoreFor = table.Column<int>(type: "int", nullable: false),
                    ScoreAgainst = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentStanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentStanding_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TournamentStanding_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_Slug",
                table: "Tournaments",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_StartDate",
                table: "Tournaments",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_Status",
                table: "Tournaments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRegistrations_Status",
                table: "TournamentRegistrations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Rankings_GameId",
                table: "Rankings",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Rankings_PlayerId_GameId",
                table: "Rankings",
                columns: new[] { "PlayerId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrizePools_TournamentId",
                table: "PrizePools",
                column: "TournamentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TournamentId",
                table: "Payments",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionReference",
                table: "Payments",
                column: "TransactionReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Slug",
                table: "Games",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedDate",
                table: "AuditLogs",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName",
                table: "AuditLogs",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrizeDistributions_PrizePoolId_Position",
                table: "PrizeDistributions",
                columns: new[] { "PrizePoolId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentStanding_PlayerId",
                table: "TournamentStanding",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentStanding_TournamentId_PlayerId",
                table: "TournamentStanding",
                columns: new[] { "TournamentId", "PlayerId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Players_PlayerId",
                table: "Payments",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Tournaments_TournamentId",
                table: "Payments",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rankings_Games_GameId",
                table: "Rankings",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentRegistrations_Players_PlayerId",
                table: "TournamentRegistrations",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Players_PlayerId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Tournaments_TournamentId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Rankings_Games_GameId",
                table: "Rankings");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentRegistrations_Players_PlayerId",
                table: "TournamentRegistrations");

            migrationBuilder.DropTable(
                name: "PrizeDistributions");

            migrationBuilder.DropTable(
                name: "TournamentStanding");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_Slug",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_StartDate",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_Status",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_TournamentRegistrations_Status",
                table: "TournamentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Rankings_GameId",
                table: "Rankings");

            migrationBuilder.DropIndex(
                name: "IX_Rankings_PlayerId_GameId",
                table: "Rankings");

            migrationBuilder.DropIndex(
                name: "IX_PrizePools_TournamentId",
                table: "PrizePools");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TournamentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TransactionReference",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Games_Slug",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CreatedDate",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityName",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Format",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Legs",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "PointsForDraw",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "PointsForLoss",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "PointsForWin",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "RegistrationDeadline",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "RegistrationOpenDate",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                table: "TournamentRegistrations");

            migrationBuilder.DropColumn(
                name: "DisqualificationReason",
                table: "TournamentRegistrations");

            migrationBuilder.DropColumn(
                name: "DisqualifiedAt",
                table: "TournamentRegistrations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TournamentRegistrations");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Rankings");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PrizePools");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentProvider",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProviderReference",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "MatchWeek",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tournaments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<bool>(
                name: "CheckedIn",
                table: "TournamentRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredDate",
                table: "TournamentRegistrations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Rankings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "FirstPlacePrize",
                table: "PrizePools",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SecondPlacePrize",
                table: "PrizePools",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ThirdPlacePrize",
                table: "PrizePools",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<Guid>(
                name: "PlayerTwoId",
                table: "Matches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PlayerOneId",
                table: "Matches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AuditLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Changes",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_Rankings_PlayerId",
                table: "Rankings",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PrizePools_TournamentId",
                table: "PrizePools",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Players_PlayerId",
                table: "Payments",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentRegistrations_Players_PlayerId",
                table: "TournamentRegistrations",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
