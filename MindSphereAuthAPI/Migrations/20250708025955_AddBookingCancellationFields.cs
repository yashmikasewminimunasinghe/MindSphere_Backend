using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindSphereAuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingCancellationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CanceledAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanceledBy",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CanceledAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CanceledBy",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsCanceled",
                table: "Bookings");
        }
    }
}
