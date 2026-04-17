using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Airline_Ticket_System.Migrations
{
    /// <inheritdoc />
    public partial class PlanEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDateTime",
                table: "Flights",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalDateTime",
                table: "Flights",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                table: "Flights",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "AT0000");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Flights",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Scheduled");

            migrationBuilder.AddColumn<string>(
                name: "Gate",
                table: "Flights",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE Flights SET ArrivalDateTime = DATEADD(MINUTE, Duration, DepartureDateTime);
                UPDATE Flights SET FlightNumber = 'AT' + RIGHT('0000' + CAST(Id AS VARCHAR(10)), 4);
                """);

            migrationBuilder.AddColumn<string>(
                name: "BookingStatus",
                table: "FlightPassengers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Confirmed");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "FlightPassengers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "FlightPassengers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentAmount",
                table: "FlightPassengers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "FlightPassengers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pnr",
                table: "FlightPassengers",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE FlightPassengers SET Pnr = RIGHT(REPLICATE('0', 6) + CAST(Id AS VARCHAR(10)), 6) WHERE Pnr IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Pnr",
                table: "FlightPassengers",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_ArrivalCity",
                table: "Flights",
                column: "ArrivalCity");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DepartureCity",
                table: "Flights",
                column: "DepartureCity");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DepartureDateTime",
                table: "Flights",
                column: "DepartureDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPassengers_CreatedAt",
                table: "FlightPassengers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPassengers_Pnr_Unique",
                table: "FlightPassengers",
                column: "Pnr",
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_FlightPassengers_FlightId_PassengerId_Unique",
                table: "FlightPassengers");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPassengers_FlightId_PassengerId_Active_Unique",
                table: "FlightPassengers",
                columns: new[] { "FlightId", "PassengerId" },
                unique: true,
                filter: "[BookingStatus] = N'Confirmed'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FlightPassengers_FlightId_PassengerId_Active_Unique",
                table: "FlightPassengers");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPassengers_FlightId_PassengerId_Unique",
                table: "FlightPassengers",
                columns: new[] { "FlightId", "PassengerId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_FlightPassengers_Pnr_Unique",
                table: "FlightPassengers");

            migrationBuilder.DropIndex(
                name: "IX_FlightPassengers_CreatedAt",
                table: "FlightPassengers");

            migrationBuilder.DropIndex(
                name: "IX_Flights_DepartureDateTime",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_Flights_DepartureCity",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_Flights_ArrivalCity",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Pnr",
                table: "FlightPassengers");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "FlightPassengers");

            migrationBuilder.DropColumn(
                name: "PaymentAmount",
                table: "FlightPassengers");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "FlightPassengers");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "FlightPassengers");

            migrationBuilder.DropColumn(
                name: "BookingStatus",
                table: "FlightPassengers");

            migrationBuilder.DropColumn(
                name: "Gate",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "FlightNumber",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalDateTime",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepartureDateTime",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");
        }
    }
}
