using System.ComponentModel.DataAnnotations;

namespace SalesTrackAcademy.Models.ViewModels;

public class AgentDashboardVm
{
    public List<AssignedCourseVm> AssignedCourses { get; set; } = [];
}

public class AssignedCourseVm
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public bool IsCompleted { get; set; }
}

public class LessonPlayerVm
{
    public int CourseId { get; set; }
    public int LessonId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public LessonType LessonType { get; set; }
    public string? ContentUrl { get; set; }
    public string? TextContent { get; set; }
    public int? PassingScorePercent { get; set; }
    public bool IsCompleted { get; set; }
    public List<QuizQuestion> QuizQuestions { get; set; } = [];
    public List<LessonComment> Comments { get; set; } = [];
}

public class SubmitQuizVm
{
    public int LessonId { get; set; }

    [Required]
    public Dictionary<int, int> AnswersByQuestionId { get; set; } = [];
}

public class CommentVm
{
    public int LessonId { get; set; }

    [Required]
    [MaxLength(800)]
    public string Body { get; set; } = string.Empty;
}
