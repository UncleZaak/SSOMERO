using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssomero.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordResetRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OtpHash = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ResetTokenHash = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    IsUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequests_Email",
                table: "PasswordResetRequests",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequests_ExpiresAt",
                table: "PasswordResetRequests",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetRequests");
        }
    }
}
