using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Infastructure.Migrations
{
    /// <inheritdoc />
    public partial class PaymentEntityRevamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TournamentRegistrationId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TournamentRegistrationId",
                table: "Payments",
                column: "TournamentRegistrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_TournamentRegistrations_TournamentRegistrationId",
                table: "Payments",
                column: "TournamentRegistrationId",
                principalTable: "TournamentRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_TournamentRegistrations_TournamentRegistrationId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TournamentRegistrationId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TournamentRegistrationId",
                table: "Payments");
        }
    }
}
