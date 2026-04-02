using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesTrackAcademy.Models;

namespace SalesTrackAcademy.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
	public DbSet<Course> Courses => Set<Course>();
	public DbSet<Lesson> Lessons => Set<Lesson>();
	public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
	public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
	public DbSet<CourseAssignment> CourseAssignments => Set<CourseAssignment>();
	public DbSet<AgentGroup> AgentGroups => Set<AgentGroup>();
	public DbSet<GroupMembership> GroupMemberships => Set<GroupMembership>();
	public DbSet<GroupCourseAssignment> GroupCourseAssignments => Set<GroupCourseAssignment>();
	public DbSet<LessonProgress> LessonProgressRecords => Set<LessonProgress>();
	public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
	public DbSet<LessonComment> LessonComments => Set<LessonComment>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.Entity<CourseAssignment>()
			.HasIndex(x => new { x.CourseId, x.AgentId })
			.IsUnique();

		builder.Entity<GroupMembership>()
			.HasIndex(x => new { x.AgentGroupId, x.AgentId })
			.IsUnique();

		builder.Entity<GroupCourseAssignment>()
			.HasIndex(x => new { x.AgentGroupId, x.CourseId })
			.IsUnique();

		builder.Entity<LessonProgress>()
			.HasIndex(x => new { x.LessonId, x.AgentId })
			.IsUnique();

		builder.Entity<Course>()
			.HasOne(x => x.CreatedBy)
			.WithMany()
			.HasForeignKey(x => x.CreatedById)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Lesson>()
			.HasOne(x => x.Course)
			.WithMany(x => x.Lessons)
			.HasForeignKey(x => x.CourseId);

		builder.Entity<QuizQuestion>()
			.HasOne(x => x.Lesson)
			.WithMany(x => x.QuizQuestions)
			.HasForeignKey(x => x.LessonId);

		builder.Entity<QuizOption>()
			.HasOne(x => x.QuizQuestion)
			.WithMany(x => x.Options)
			.HasForeignKey(x => x.QuizQuestionId);

		builder.Entity<CourseAssignment>()
			.HasOne(x => x.Course)
			.WithMany(x => x.Assignments)
			.HasForeignKey(x => x.CourseId);

		builder.Entity<CourseAssignment>()
			.HasOne(x => x.Agent)
			.WithMany()
			.HasForeignKey(x => x.AgentId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<GroupMembership>()
			.HasOne(x => x.AgentGroup)
			.WithMany(x => x.Memberships)
			.HasForeignKey(x => x.AgentGroupId);

		builder.Entity<GroupMembership>()
			.HasOne(x => x.Agent)
			.WithMany()
			.HasForeignKey(x => x.AgentId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<GroupCourseAssignment>()
			.HasOne(x => x.AgentGroup)
			.WithMany(x => x.CourseAssignments)
			.HasForeignKey(x => x.AgentGroupId);

		builder.Entity<GroupCourseAssignment>()
			.HasOne(x => x.Course)
			.WithMany(x => x.GroupAssignments)
			.HasForeignKey(x => x.CourseId);

		builder.Entity<LessonProgress>()
			.HasOne(x => x.Lesson)
			.WithMany(x => x.ProgressRecords)
			.HasForeignKey(x => x.LessonId);

		builder.Entity<LessonProgress>()
			.HasOne(x => x.Agent)
			.WithMany()
			.HasForeignKey(x => x.AgentId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<QuizAttempt>()
			.HasOne(x => x.Lesson)
			.WithMany()
			.HasForeignKey(x => x.LessonId);

		builder.Entity<QuizAttempt>()
			.HasOne(x => x.Agent)
			.WithMany()
			.HasForeignKey(x => x.AgentId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<LessonComment>()
			.HasOne(x => x.Lesson)
			.WithMany(x => x.Comments)
			.HasForeignKey(x => x.LessonId);

		builder.Entity<LessonComment>()
			.HasOne(x => x.Agent)
			.WithMany()
			.HasForeignKey(x => x.AgentId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
