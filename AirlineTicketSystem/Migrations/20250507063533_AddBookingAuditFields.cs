using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Airline_Ticket_System.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Passengers",
                newName: "FamilyName");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FlightPassengers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "FlightPassengers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlightPassengers_CreatedByUserId",
                table: "FlightPassengers",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlightPassengers_AspNetUsers_CreatedByUserId",
                table: "FlightPassengers",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlightPassengers_AspNetUsers_CreatedByUserId",
                table: "FlightPassengers");

            migrationBuilder.DropIndex(
                name: "IX_FlightPassengers_CreatedByUserId",
                table: "FlightPassengers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FlightPassengers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "FlightPassengers");

            migrationBuilder.RenameColumn(
                name: "FamilyName",
                table: "Passengers",
                newName: "LastName");
        }
    }
}
