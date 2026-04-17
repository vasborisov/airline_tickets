using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Airline_Ticket_System.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FlightPassengers_FlightId",
                table: "FlightPassengers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FlightPassengers_FlightId",
                table: "FlightPassengers",
                column: "FlightId");
        }
    }
}
