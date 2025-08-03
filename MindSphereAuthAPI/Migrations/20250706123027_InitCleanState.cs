using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindSphereAuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitCleanState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_ClientId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Counsellors_CounsellorId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizResponses_QuizQuestions_QuizQuestionId",
                table: "QuizResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuizOptions_QuizQuestionId_Text",
                table: "QuizOptions");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "QuizOptions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_QuizOptions_QuizQuestionId",
                table: "QuizOptions",
                column: "QuizQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_ClientId",
                table: "Bookings",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Counsellors_CounsellorId",
                table: "Bookings",
                column: "CounsellorId",
                principalTable: "Counsellors",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizResponses_QuizQuestions_QuizQuestionId",
                table: "QuizResponses",
                column: "QuizQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_ClientId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Counsellors_CounsellorId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizResponses_QuizQuestions_QuizQuestionId",
                table: "QuizResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuizOptions_QuizQuestionId",
                table: "QuizOptions");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "QuizOptions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_QuizOptions_QuizQuestionId_Text",
                table: "QuizOptions",
                columns: new[] { "QuizQuestionId", "Text" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_ClientId",
                table: "Bookings",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Counsellors_CounsellorId",
                table: "Bookings",
                column: "CounsellorId",
                principalTable: "Counsellors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizResponses_QuizQuestions_QuizQuestionId",
                table: "QuizResponses",
                column: "QuizQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
