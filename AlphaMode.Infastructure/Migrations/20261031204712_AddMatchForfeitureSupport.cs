using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Infastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchForfeitureSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ForfeitNotes",
                table: "Matches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ForfeitReason",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ForfeitedAt",
                table: "Matches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ForfeitingPlayerId",
                table: "Matches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResultType",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForfeitNotes",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ForfeitReason",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ForfeitedAt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ForfeitingPlayerId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ResultType",
                table: "Matches");
        }
    }
}
