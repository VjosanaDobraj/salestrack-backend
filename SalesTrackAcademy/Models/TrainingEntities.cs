using System.ComponentModel.DataAnnotations;

namespace SalesTrackAcademy.Models;

public enum LessonType
{
    Video = 1,
    Audio = 2,
    Pdf = 3,
    Text = 4
}

public class Course
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [Required]
    public string CreatedById { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ApplicationUser? CreatedBy { get; set; }

    public List<Lesson> Lessons { get; set; } = [];
    public List<CourseAssignment> Assignments { get; set; } = [];
    public List<GroupCourseAssignment> GroupAssignments { get; set; } = [];
}

public class Lesson
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    [Required]
    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    public LessonType LessonType { get; set; }

    [MaxLength(800)]
    public string? ContentUrl { get; set; }

    public string? TextContent { get; set; }

    public int SortOrder { get; set; }

    public int? PassingScorePercent { get; set; }

    public Course? Course { get; set; }
    public List<QuizQuestion> QuizQuestions { get; set; } = [];
    public List<LessonComment> Comments { get; set; } = [];
    public List<LessonProgress> ProgressRecords { get; set; } = [];
}

public class QuizQuestion
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    [Required]
    [MaxLength(350)]
    public string Prompt { get; set; } = string.Empty;

    public Lesson? Lesson { get; set; }
    public List<QuizOption> Options { get; set; } = [];
}

public class QuizOption
{
    public int Id { get; set; }

    public int QuizQuestionId { get; set; }

    [Required]
    [MaxLength(220)]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public QuizQuestion? QuizQuestion { get; set; }
}

public class CourseAssignment
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    [Required]
    public string AgentId { get; set; } = string.Empty;

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    public Course? Course { get; set; }
    public ApplicationUser? Agent { get; set; }
}

public class AgentGroup
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public List<GroupMembership> Memberships { get; set; } = [];
    public List<GroupCourseAssignment> CourseAssignments { get; set; } = [];
}

public class GroupMembership
{
    public int Id { get; set; }

    public int AgentGroupId { get; set; }

    [Required]
    public string AgentId { get; set; } = string.Empty;

    public AgentGroup? AgentGroup { get; set; }
    public ApplicationUser? Agent { get; set; }
}

public class GroupCourseAssignment
{
    public int Id { get; set; }

    public int AgentGroupId { get; set; }

    public int CourseId { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    public AgentGroup? AgentGroup { get; set; }
    public Course? Course { get; set; }
}

public class LessonProgress
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    [Required]
    public string AgentId { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public int TimeSpentMinutes { get; set; }

    public Lesson? Lesson { get; set; }
    public ApplicationUser? Agent { get; set; }
}

public class QuizAttempt
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    [Required]
    public string AgentId { get; set; } = string.Empty;

    public int ScorePercent { get; set; }

    public bool Passed { get; set; }

    public DateTime AttemptedAtUtc { get; set; } = DateTime.UtcNow;

    public Lesson? Lesson { get; set; }
    public ApplicationUser? Agent { get; set; }
}

public class LessonComment
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    [Required]
    public string AgentId { get; set; } = string.Empty;

    [Required]
    [MaxLength(800)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Lesson? Lesson { get; set; }
    public ApplicationUser? Agent { get; set; }
}
