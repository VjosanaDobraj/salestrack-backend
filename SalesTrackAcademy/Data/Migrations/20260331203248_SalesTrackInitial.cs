using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesTrackAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesTrackInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AgentGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AgentGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    AgentId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMemberships_AgentGroups_AgentGroupId",
                        column: x => x.AgentGroupId,
                        principalTable: "AgentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMemberships_AspNetUsers_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    AgentId = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseAssignments_AspNetUsers_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseAssignments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupCourseAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AgentGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupCourseAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupCourseAssignments_AgentGroups_AgentGroupId",
                        column: x => x.AgentGroupId,
                        principalTable: "AgentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupCourseAssignments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    LessonType = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentUrl = table.Column<string>(type: "TEXT", maxLength: 800, nullable: true),
                    TextContent = table.Column<string>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    PassingScorePercent = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lessons_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LessonId = table.Column<int>(type: "INTEGER", nullable: false),
                    AgentId = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", maxLength: 800, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonComments_AspNetUsers_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonComments_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonProgressRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LessonId = table.Column<int>(type: "INTEGER", nullable: false),
                    AgentId = table.Column<string>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeSpentMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonProgressRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonProgressRecords_AspNetUsers_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonProgressRecords_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LessonId = table.Column<int>(type: "INTEGER", nullable: false),
                    AgentId = table.Column<string>(type: "TEXT", nullable: false),
                    ScorePercent = table.Column<int>(type: "INTEGER", nullable: false),
                    AttemptedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_AspNetUsers_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LessonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", maxLength: 350, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuizQuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    OptionText = table.Column<string>(type: "TEXT", maxLength: 220, nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizOptions_QuizQuestions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseAssignments_AgentId",
                table: "CourseAssignments",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAssignments_CourseId_AgentId",
                table: "CourseAssignments",
                columns: new[] { "CourseId", "AgentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CreatedById",
                table: "Courses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCourseAssignments_AgentGroupId_CourseId",
                table: "GroupCourseAssignments",
                columns: new[] { "AgentGroupId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupCourseAssignments_CourseId",
                table: "GroupCourseAssignments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_AgentGroupId_AgentId",
                table: "GroupMemberships",
                columns: new[] { "AgentGroupId", "AgentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_AgentId",
                table: "GroupMemberships",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonComments_AgentId",
                table: "LessonComments",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonComments_LessonId",
                table: "LessonComments",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgressRecords_AgentId",
                table: "LessonProgressRecords",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgressRecords_LessonId_AgentId",
                table: "LessonProgressRecords",
                columns: new[] { "LessonId", "AgentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_CourseId",
                table: "Lessons",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_AgentId",
                table: "QuizAttempts",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_LessonId",
                table: "QuizAttempts",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizOptions_QuizQuestionId",
                table: "QuizOptions",
                column: "QuizQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_LessonId",
                table: "QuizQuestions",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseAssignments");

            migrationBuilder.DropTable(
                name: "GroupCourseAssignments");

            migrationBuilder.DropTable(
                name: "GroupMemberships");

            migrationBuilder.DropTable(
                name: "LessonComments");

            migrationBuilder.DropTable(
                name: "LessonProgressRecords");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "QuizOptions");

            migrationBuilder.DropTable(
                name: "AgentGroups");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "Lessons");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");
        }
    }
}
