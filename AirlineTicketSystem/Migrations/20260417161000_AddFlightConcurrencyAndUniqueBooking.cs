using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Airline_Ticket_System.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightConcurrencyAndUniqueBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Flights",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_FlightPassengers_FlightId_PassengerId_Unique",
                table: "FlightPassengers",
                columns: new[] { "FlightId", "PassengerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FlightPassengers_FlightId_PassengerId_Unique",
                table: "FlightPassengers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Flights");
        }
    }
}
