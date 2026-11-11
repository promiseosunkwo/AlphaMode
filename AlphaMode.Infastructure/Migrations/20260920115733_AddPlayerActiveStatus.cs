using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Infastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerActiveStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentStanding_Players_PlayerId",
                table: "TournamentStanding");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentStanding_Tournaments_TournamentId",
                table: "TournamentStanding");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentStanding",
                table: "TournamentStanding");

            migrationBuilder.RenameTable(
                name: "TournamentStanding",
                newName: "TournamentStandings");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentStanding_TournamentId_PlayerId",
                table: "TournamentStandings",
                newName: "IX_TournamentStandings_TournamentId_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentStanding_PlayerId",
                table: "TournamentStandings",
                newName: "IX_TournamentStandings_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentStandings",
                table: "TournamentStandings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentStandings_Players_PlayerId",
                table: "TournamentStandings",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentStandings_Tournaments_TournamentId",
                table: "TournamentStandings",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentStandings_Players_PlayerId",
                table: "TournamentStandings");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentStandings_Tournaments_TournamentId",
                table: "TournamentStandings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentStandings",
                table: "TournamentStandings");

            migrationBuilder.RenameTable(
                name: "TournamentStandings",
                newName: "TournamentStanding");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentStandings_TournamentId_PlayerId",
                table: "TournamentStanding",
                newName: "IX_TournamentStanding_TournamentId_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentStandings_PlayerId",
                table: "TournamentStanding",
                newName: "IX_TournamentStanding_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentStanding",
                table: "TournamentStanding",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentStanding_Players_PlayerId",
                table: "TournamentStanding",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentStanding_Tournaments_TournamentId",
                table: "TournamentStanding",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
