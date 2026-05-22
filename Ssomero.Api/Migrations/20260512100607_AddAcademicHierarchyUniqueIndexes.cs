using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssomero.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicHierarchyUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Curricula_Programs_ProgramId",
                table: "Curricula");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Faculties_FacultyId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_Universities_UniversityId",
                table: "Faculties");

            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Departments_DepartmentId",
                table: "Programs");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Name",
                table: "Universities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Programs_Name_DepartmentId",
                table: "Programs",
                columns: new[] { "Name", "DepartmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_Name_UniversityId",
                table: "Faculties",
                columns: new[] { "Name", "UniversityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name_FacultyId",
                table: "Departments",
                columns: new[] { "Name", "FacultyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Curricula_CourseCode_ProgramId",
                table: "Curricula",
                columns: new[] { "CourseCode", "ProgramId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Curricula_Programs_ProgramId",
                table: "Curricula",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Faculties_FacultyId",
                table: "Departments",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Universities_UniversityId",
                table: "Faculties",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Departments_DepartmentId",
                table: "Programs",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Curricula_Programs_ProgramId",
                table: "Curricula");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Faculties_FacultyId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_Universities_UniversityId",
                table: "Faculties");

            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Departments_DepartmentId",
                table: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_Universities_Name",
                table: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_Programs_Name_DepartmentId",
                table: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_Faculties_Name_UniversityId",
                table: "Faculties");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Name_FacultyId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Curricula_CourseCode_ProgramId",
                table: "Curricula");

            migrationBuilder.AddForeignKey(
                name: "FK_Curricula_Programs_ProgramId",
                table: "Curricula",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Faculties_FacultyId",
                table: "Departments",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Universities_UniversityId",
                table: "Faculties",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Departments_DepartmentId",
                table: "Programs",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
