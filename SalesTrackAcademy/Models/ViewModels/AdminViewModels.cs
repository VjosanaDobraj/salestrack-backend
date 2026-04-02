using System.ComponentModel.DataAnnotations;

namespace SalesTrackAcademy.Models.ViewModels;

public class AdminDashboardVm
{
    public int TotalCourses { get; set; }
    public int TotalAgents { get; set; }
    public double AvgCompletionRate { get; set; }
    public double AvgQuizScore { get; set; }
    public int TotalComments { get; set; }
    public List<AgentProgressVm> AgentProgress { get; set; } = [];
}

public class AgentProgressVm
{
    public string AgentName { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    public int AssignedCourses { get; set; }
    public int CompletedCourses { get; set; }
    public int CompletionPercent { get; set; }
    public int BestQuizScore { get; set; }
}

public class CourseEditVm
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Thumbnail URL")]
    [MaxLength(500)]
    public string ThumbnailUrl { get; set; } = string.Empty;
}

public class LessonEditVm
{
    public int CourseId { get; set; }

    [Required]
    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    public LessonType LessonType { get; set; }

    [Display(Name = "Media or Document URL")]
    [MaxLength(800)]
    public string? ContentUrl { get; set; }

    [Display(Name = "Text Content")]
    public string? TextContent { get; set; }

    [Display(Name = "Passing Score %")]
    [Range(1, 100)]
    public int? PassingScorePercent { get; set; }
}

public class QuizQuestionVm
{
    public int LessonId { get; set; }

    [Required]
    [MaxLength(350)]
    public string Prompt { get; set; } = string.Empty;

    [Required]
    public string OptionA { get; set; } = string.Empty;

    [Required]
    public string OptionB { get; set; } = string.Empty;

    [Required]
    public string OptionC { get; set; } = string.Empty;

    [Required]
    public string OptionD { get; set; } = string.Empty;

    [Range(1, 4)]
    public int CorrectOption { get; set; }
}

public class AssignmentVm
{
    public int CourseId { get; set; }
    public List<string> AgentIds { get; set; } = [];
    public List<int> GroupIds { get; set; } = [];
}

public class GroupCreateVm
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public List<string> AgentIds { get; set; } = [];
}
