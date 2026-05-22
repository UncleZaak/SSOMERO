using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssomero.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixAttendanceUniqueIndex_StudentSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendances_StudentId_ClassId_Date",
                table: "Attendances");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId_SessionId",
                table: "Attendances",
                columns: new[] { "StudentId", "SessionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendances_StudentId_SessionId",
                table: "Attendances");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId_ClassId_Date",
                table: "Attendances",
                columns: new[] { "StudentId", "ClassId", "Date" },
                unique: true);
        }
    }
}
