using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseHub.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class ActiveEnrollmentUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_OneActivePerLearner",
                table: "Enrollments",
                columns: new[] { "CourseId", "LearnerId" },
                unique: true,
                filter: "\"Status\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrollments_OneActivePerLearner",
                table: "Enrollments");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                column: "CourseId");
        }
    }
}
