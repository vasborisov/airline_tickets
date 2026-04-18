using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicketSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToPassenger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Passengers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Passengers");
        }
    }
}