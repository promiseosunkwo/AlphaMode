using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Infastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureTournamentRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentRegistrations_Tournaments_TournamentId",
                table: "TournamentRegistrations");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "TournamentRegistrations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "DisqualificationReason",
                table: "TournamentRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRegistrations_TournamentId",
                table: "TournamentRegistrations",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentRegistrations_Tournaments_TournamentId",
                table: "TournamentRegistrations",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentRegistrations_Tournaments_TournamentId",
                table: "TournamentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_TournamentRegistrations_TournamentId",
                table: "TournamentRegistrations");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "TournamentRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DisqualificationReason",
                table: "TournamentRegistrations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentRegistrations_Tournaments_TournamentId",
                table: "TournamentRegistrations",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
