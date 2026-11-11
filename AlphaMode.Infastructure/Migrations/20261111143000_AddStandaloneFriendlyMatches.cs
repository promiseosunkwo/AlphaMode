using AlphaMode.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Infastructure.Migrations;

// This migration removes the old requirement that every match belong to a tournament.
// It also backfills GameId before making it required, preserving all existing fixtures.
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261111143000_AddStandaloneFriendlyMatches")]
public partial class AddStandaloneFriendlyMatches : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "GameId", table: "Matches", type: "uniqueidentifier", nullable: true);

        migrationBuilder.Sql("""
            UPDATE m
            SET m.GameId = t.GameId,
                m.Type = 1,
                m.IsRanked = 1
            FROM Matches AS m
            INNER JOIN Tournaments AS t ON t.Id = m.TournamentId;
            """);

        migrationBuilder.AlterColumn<Guid>(name: "GameId", table: "Matches", type: "uniqueidentifier", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);
        migrationBuilder.AlterColumn<Guid>(name: "TournamentId", table: "Matches", type: "uniqueidentifier", nullable: true, oldClrType: typeof(Guid), oldType: "uniqueidentifier");
        migrationBuilder.CreateIndex(name: "IX_Matches_GameId", table: "Matches", column: "GameId");
        migrationBuilder.CreateIndex(name: "IX_Matches_Type_Status_ScheduledAt", table: "Matches", columns: new[] { "Type", "Status", "ScheduledAt" });
        migrationBuilder.AddForeignKey(name: "FK_Matches_Games_GameId", table: "Matches", column: "GameId", principalTable: "Games", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Downgrade cannot represent standalone matches, so remove them before TournamentId becomes required.
        migrationBuilder.Sql("DELETE FROM Matches WHERE TournamentId IS NULL;");
        migrationBuilder.DropForeignKey(name: "FK_Matches_Games_GameId", table: "Matches");
        migrationBuilder.DropIndex(name: "IX_Matches_GameId", table: "Matches");
        migrationBuilder.DropIndex(name: "IX_Matches_Type_Status_ScheduledAt", table: "Matches");
        migrationBuilder.DropColumn(name: "GameId", table: "Matches");
        migrationBuilder.AlterColumn<Guid>(name: "TournamentId", table: "Matches", type: "uniqueidentifier", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);
    }
}

