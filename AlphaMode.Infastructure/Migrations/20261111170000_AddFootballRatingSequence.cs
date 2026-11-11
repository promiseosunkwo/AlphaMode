using AlphaMode.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Infastructure.Migrations;

// Elo is order-dependent. A SQL sequence provides a concurrency-safe position shared by
// both player history rows for one match and later supports deterministic correction replay.
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261111170000_AddFootballRatingSequence")]
public partial class AddFootballRatingSequence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSequence<long>(name: "FootballRatingSequence");
        migrationBuilder.DropIndex(name: "IX_FootballRatingHistories_ProcessingSequence", table: "FootballRatingHistories");
        migrationBuilder.CreateIndex(name: "IX_FootballRatingHistories_GameId_ProcessingSequence", table: "FootballRatingHistories", columns: new[] { "GameId", "ProcessingSequence" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_FootballRatingHistories_GameId_ProcessingSequence", table: "FootballRatingHistories");
        migrationBuilder.CreateIndex(name: "IX_FootballRatingHistories_ProcessingSequence", table: "FootballRatingHistories", column: "ProcessingSequence");
        migrationBuilder.DropSequence(name: "FootballRatingSequence");
    }
}

