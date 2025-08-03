using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindSphereAuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToCounsellor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Counsellors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Counsellors");
        }
    }
}
