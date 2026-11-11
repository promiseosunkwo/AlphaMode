using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Infastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchPenaltyShootoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerOnePenaltyScore",
                table: "MatchScores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayerTwoPenaltyScore",
                table: "MatchScores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WentToExtraTime",
                table: "MatchScores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRanked",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerOnePenaltyScore",
                table: "MatchScores");

            migrationBuilder.DropColumn(
                name: "PlayerTwoPenaltyScore",
                table: "MatchScores");

            migrationBuilder.DropColumn(
                name: "WentToExtraTime",
                table: "MatchScores");

            migrationBuilder.DropColumn(
                name: "IsRanked",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Matches");
        }
    }
}
