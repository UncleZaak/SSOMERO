using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssomero.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddClassElections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassElections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClassId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedByStudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    WinnerStudentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassElections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassElections_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassElectionCandidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ElectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassElectionCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassElectionCandidates_ClassElections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "ClassElections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassElectionCandidates_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassElectionVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ElectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VoterStudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CandidateStudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassElectionVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassElectionVotes_ClassElections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "ClassElections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassElectionVotes_Students_VoterStudentId",
                        column: x => x.VoterStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassElectionCandidates_ElectionId_StudentId",
                table: "ClassElectionCandidates",
                columns: new[] { "ElectionId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassElectionCandidates_StudentId",
                table: "ClassElectionCandidates",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassElections_ClassId_Status",
                table: "ClassElections",
                columns: new[] { "ClassId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassElectionVotes_ElectionId_VoterStudentId",
                table: "ClassElectionVotes",
                columns: new[] { "ElectionId", "VoterStudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassElectionVotes_VoterStudentId",
                table: "ClassElectionVotes",
                column: "VoterStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassElectionCandidates");

            migrationBuilder.DropTable(
                name: "ClassElectionVotes");

            migrationBuilder.DropTable(
                name: "ClassElections");
        }
    }
}
