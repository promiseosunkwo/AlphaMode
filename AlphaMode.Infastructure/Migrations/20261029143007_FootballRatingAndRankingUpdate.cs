using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Infastructure.Migrations
{
    /// <inheritdoc />
    public partial class FootballRatingAndRankingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Draws",
                table: "Rankings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "EloRating",
                table: "Rankings",
                type: "decimal(12,4)",
                precision: 12,
                scale: 4,
                nullable: false,
                defaultValue: 1000m);

            migrationBuilder.AddColumn<int>(
                name: "GoalsAgainst",
                table: "Rankings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GoalsFor",
                table: "Rankings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRankedMatchAt",
                table: "Rankings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Losses",
                table: "Rankings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MatchesPlayed",
                table: "Rankings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Wins",
                table: "Rankings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FootballRatingHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpponentPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RatingBefore = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    RatingChange = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    RatingAfter = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    GoalsFor = table.Column<int>(type: "int", nullable: false),
                    GoalsAgainst = table.Column<int>(type: "int", nullable: false),
                    ResultScore = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    ProcessingSequence = table.Column<long>(type: "bigint", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballRatingHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballRatingHistories_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FootballRatingHistories_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FootballRatingHistories_Players_OpponentPlayerId",
                        column: x => x.OpponentPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FootballRatingHistories_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rankings_GameId_EloRating",
                table: "Rankings",
                columns: new[] { "GameId", "EloRating" });

            migrationBuilder.CreateIndex(
                name: "IX_FootballRatingHistories_GameId",
                table: "FootballRatingHistories",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballRatingHistories_MatchId_PlayerId",
                table: "FootballRatingHistories",
                columns: new[] { "MatchId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballRatingHistories_OpponentPlayerId",
                table: "FootballRatingHistories",
                column: "OpponentPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballRatingHistories_PlayerId",
                table: "FootballRatingHistories",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballRatingHistories_ProcessingSequence",
                table: "FootballRatingHistories",
                column: "ProcessingSequence");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FootballRatingHistories");

            migrationBuilder.DropIndex(
                name: "IX_Rankings_GameId_EloRating",
                table: "Rankings");

            migrationBuilder.DropColumn(
                name: "Draws",
                table: "Rankings");

            migrationBuilder.DropColumn(
                name: "EloRating",
                table: "Rankings");

            migrationBuilder.DropColumn(
                name: "GoalsAgainst",
                table: "Rankings");

            migrationBuilder.DropColumn(
                name: "GoalsFor",
                table: "Rankings");

            migrationBuilder.DropColumn(
                name: "LastRankedMatchAt",
                table: "Rankings");

            migrationBuilder.DropColumn(
                name: "Losses",
                table: "Rankings");

            migrationBuilder.DropColumn(
                name: "MatchesPlayed",
                table: "Rankings");

            migrationBuilder.DropColumn(
                name: "Wins",
                table: "Rankings");
        }
    }
}
