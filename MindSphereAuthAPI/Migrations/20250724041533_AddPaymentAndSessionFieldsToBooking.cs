using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindSphereAuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAndSessionFieldsToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionLink",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WebhookProcessed",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SessionLink",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "WebhookProcessed",
                table: "Bookings");
        }
    }
}
