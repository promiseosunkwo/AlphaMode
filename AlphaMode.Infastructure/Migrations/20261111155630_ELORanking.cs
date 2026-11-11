using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Infastructure.Migrations
{
    /// <inheritdoc />
    public partial class ELORanking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FootballRatingHistories_GameId",
                table: "FootballRatingHistories");

            migrationBuilder.DropSequence(
                name: "FootballRatingSequence");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "FootballRatingSequence");

            migrationBuilder.CreateIndex(
                name: "IX_FootballRatingHistories_GameId",
                table: "FootballRatingHistories",
                column: "GameId");
        }
    }
}
