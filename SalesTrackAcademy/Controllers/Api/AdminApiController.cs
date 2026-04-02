using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesTrackAcademy.Data;
using SalesTrackAcademy.Models;

namespace SalesTrackAcademy.Controllers.Api;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminApiController(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    // ─────────────────────── DASHBOARD ───────────────────────

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var agents = await userManager.GetUsersInRoleAsync("Agent");
        var courses = await db.Courses.Include(c => c.Lessons).ToListAsync();
        var progress = await db.LessonProgressRecords.Where(p => p.IsCompleted).ToListAsync();
        var attempts = await db.QuizAttempts.ToListAsync();
        var comments = await db.LessonComments.CountAsync();
        var totalLessons = courses.Sum(c => c.Lessons.Count);

        var agentProgress = new List<object>();
        foreach (var agent in agents)
        {
            var directIds = await db.CourseAssignments.Where(x => x.AgentId == agent.Id).Select(x => x.CourseId).ToListAsync();
            var groupIds = await db.GroupMemberships.Where(x => x.AgentId == agent.Id).Select(x => x.AgentGroupId).ToListAsync();
            var groupCourseIds = await db.GroupCourseAssignments.Where(x => groupIds.Contains(x.AgentGroupId)).Select(x => x.CourseId).ToListAsync();
            var allCourseIds = directIds.Union(groupCourseIds).Distinct().ToList();
            var lessonIds = await db.Lessons.Where(l => allCourseIds.Contains(l.CourseId)).Select(l => l.Id).ToListAsync();
            var done = progress.Count(p => p.AgentId == agent.Id && lessonIds.Contains(p.LessonId));
            var pct = lessonIds.Count == 0 ? 0 : Math.Round((double)done / lessonIds.Count * 100, 1);
            var quizScore = attempts.Where(a => a.AgentId == agent.Id).Select(a => (double?)a.ScorePercent).Average() ?? 0;

            agentProgress.Add(new
            {
                id = agent.Id,
                name = string.IsNullOrWhiteSpace(agent.FullName) ? agent.Email : agent.FullName,
                email = agent.Email,
                completionRate = pct,
                avgQuizScore = Math.Round(quizScore, 1),
                assignedCourses = allCourseIds.Count
            });
        }

        return Ok(new
        {
            totalCourses = courses.Count,
            totalAgents = agents.Count,
            completionRate = totalLessons == 0 || agents.Count == 0 ? 0 : Math.Round((double)progress.Count / (totalLessons * agents.Count) * 100, 1),
            avgQuizScore = attempts.Count == 0 ? 0 : Math.Round(attempts.Average(a => a.ScorePercent), 1),
            totalComments = comments,
            agents = agentProgress
        });
    }

    // ─────────────────────── COURSES ───────────────────────

    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses()
    {
        var courses = await db.Courses
            .Include(c => c.Lessons.OrderBy(l => l.SortOrder))
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync();

        return Ok(courses.Select(c => new
        {
            id = c.Id,
            title = c.Title,
            description = c.Description,
            thumbnailUrl = c.ThumbnailUrl,
            lessonCount = c.Lessons.Count,
            createdAt = c.CreatedAtUtc
        }));
    }

    [HttpGet("courses/{id}")]
    public async Task<IActionResult> GetCourse(int id)
    {
        var course = await db.Courses
            .Include(c => c.Lessons.OrderBy(l => l.SortOrder))
                .ThenInclude(l => l.QuizQuestions.OrderBy(q => q.Id))
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course is null) return NotFound();

        return Ok(new
        {
            id = course.Id,
            title = course.Title,
            description = course.Description,
            thumbnailUrl = course.ThumbnailUrl,
            createdAt = course.CreatedAtUtc,
            lessons = course.Lessons.Select(l => new
            {
                id = l.Id,
                title = l.Title,
                lessonType = l.LessonType.ToString(),
                contentUrl = l.ContentUrl,
                textContent = l.TextContent,
                sortOrder = l.SortOrder,
                passingScorePercent = l.PassingScorePercent,
                quizQuestions = l.QuizQuestions.Select(q => new
                {
                    id = q.Id,
                    prompt = q.Prompt,
                    options = q.Options.Select(o => new { id = o.Id, text = o.OptionText, isCorrect = o.IsCorrect })
                })
            })
        });
    }

    public record CourseDto(string Title, string? Description, string? ThumbnailUrl);

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CourseDto dto)
    {
        var admin = await userManager.GetUserAsync(User);
        if (admin is null) return Challenge();

        var course = new Course { Title = dto.Title, Description = dto.Description, ThumbnailUrl = dto.ThumbnailUrl, CreatedById = admin.Id };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return Ok(new { id = course.Id, title = course.Title });
    }

    [HttpPut("courses/{id}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseDto dto)
    {
        var course = await db.Courses.FindAsync(id);
        if (course is null) return NotFound();
        course.Title = dto.Title;
        course.Description = dto.Description;
        course.ThumbnailUrl = dto.ThumbnailUrl;
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("courses/{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await db.Courses.FindAsync(id);
        if (course is null) return NotFound();
        db.Courses.Remove(course);
        await db.SaveChangesAsync();
        return Ok();
    }

    // ─────────────────────── LESSONS ───────────────────────

    public record LessonDto(string Title, string LessonType, string? ContentUrl, string? TextContent, int? PassingScorePercent);

    [HttpPost("courses/{courseId}/lessons")]
    public async Task<IActionResult> AddLesson(int courseId, [FromBody] LessonDto dto)
    {
        var sortOrder = await db.Lessons.Where(l => l.CourseId == courseId).Select(l => (int?)l.SortOrder).MaxAsync() ?? 0;
        if (!Enum.TryParse<LessonType>(dto.LessonType, true, out var type)) return BadRequest("Invalid lesson type.");
        var lesson = new Lesson
        {
            CourseId = courseId,
            Title = dto.Title,
            LessonType = type,
            ContentUrl = dto.ContentUrl,
            TextContent = dto.TextContent,
            PassingScorePercent = dto.PassingScorePercent ?? 80,
            SortOrder = sortOrder + 1
        };
        db.Lessons.Add(lesson);
        await db.SaveChangesAsync();
        return Ok(new { id = lesson.Id });
    }

    [HttpPut("lessons/{id}")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] LessonDto dto)
    {
        var lesson = await db.Lessons.FindAsync(id);
        if (lesson is null) return NotFound();
        if (!Enum.TryParse<LessonType>(dto.LessonType, true, out var type)) return BadRequest("Invalid lesson type.");
        lesson.Title = dto.Title;
        lesson.LessonType = type;
        lesson.ContentUrl = dto.ContentUrl;
        lesson.TextContent = dto.TextContent;
        lesson.PassingScorePercent = dto.PassingScorePercent ?? 80;
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("lessons/{id}")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        var lesson = await db.Lessons.FindAsync(id);
        if (lesson is null) return NotFound();
        db.Lessons.Remove(lesson);
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("lessons/reorder")]
    public async Task<IActionResult> ReorderLessons([FromBody] List<int> orderedIds)
    {
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var lesson = await db.Lessons.FindAsync(orderedIds[i]);
            if (lesson is not null) lesson.SortOrder = i + 1;
        }
        await db.SaveChangesAsync();
        return Ok();
    }

    // ─────────────────────── QUIZ ───────────────────────

    public record QuizQuestionDto(string Prompt, string OptionA, string OptionB, string OptionC, string OptionD, int CorrectOption);

    [HttpPost("lessons/{lessonId}/quiz")]
    public async Task<IActionResult> AddQuizQuestion(int lessonId, [FromBody] QuizQuestionDto dto)
    {
        var question = new QuizQuestion
        {
            LessonId = lessonId,
            Prompt = dto.Prompt,
            Options =
            [
                new QuizOption { OptionText = dto.OptionA, IsCorrect = dto.CorrectOption == 1 },
                new QuizOption { OptionText = dto.OptionB, IsCorrect = dto.CorrectOption == 2 },
                new QuizOption { OptionText = dto.OptionC, IsCorrect = dto.CorrectOption == 3 },
                new QuizOption { OptionText = dto.OptionD, IsCorrect = dto.CorrectOption == 4 }
            ]
        };
        db.QuizQuestions.Add(question);
        await db.SaveChangesAsync();
        return Ok(new { id = question.Id });
    }

    [HttpDelete("quizquestions/{id}")]
    public async Task<IActionResult> DeleteQuizQuestion(int id)
    {
        var q = await db.QuizQuestions.FindAsync(id);
        if (q is null) return NotFound();
        db.QuizQuestions.Remove(q);
        await db.SaveChangesAsync();
        return Ok();
    }

    // ─────────────────────── AGENTS ───────────────────────

    [HttpGet("agents")]
    public async Task<IActionResult> GetAgents()
    {
        var agents = await userManager.GetUsersInRoleAsync("Agent");
        return Ok(agents.Select(a => new { id = a.Id, email = a.Email, fullName = a.FullName }));
    }

    [HttpGet("agents/{id}")]
    public async Task<IActionResult> GetAgentDetail(string id)
    {
        var agent = await userManager.FindByIdAsync(id);
        if (agent is null) return NotFound();

        var directIds = await db.CourseAssignments.Where(x => x.AgentId == id).Select(x => x.CourseId).ToListAsync();
        var groupIds = await db.GroupMemberships.Where(x => x.AgentId == id).Select(x => x.AgentGroupId).ToListAsync();
        var groupCourseIds = await db.GroupCourseAssignments.Where(x => groupIds.Contains(x.AgentGroupId)).Select(x => x.CourseId).ToListAsync();
        var allCourseIds = directIds.Union(groupCourseIds).Distinct().ToList();

        var courses = await db.Courses.Include(c => c.Lessons).Where(c => allCourseIds.Contains(c.Id)).ToListAsync();
        var progressRecords = await db.LessonProgressRecords.Where(p => p.AgentId == id).ToListAsync();
        var attempts = await db.QuizAttempts.Where(a => a.AgentId == id).ToListAsync();

        var courseDetails = courses.Select(c =>
        {
            var lessonIds = c.Lessons.Select(l => l.Id).ToList();
            var completedCount = progressRecords.Count(p => p.IsCompleted && lessonIds.Contains(p.LessonId));
            var pct = lessonIds.Count == 0 ? 0 : (int)Math.Round((double)completedCount / lessonIds.Count * 100);
            return new
            {
                courseId = c.Id,
                title = c.Title,
                progress = pct,
                completed = pct == 100,
                lessonCount = lessonIds.Count,
                completedLessons = completedCount
            };
        }).ToList();

        return Ok(new
        {
            id = agent.Id,
            name = string.IsNullOrWhiteSpace(agent.FullName) ? agent.Email : agent.FullName,
            email = agent.Email,
            assignedCourses = allCourseIds.Count,
            completionRate = courseDetails.Count == 0 ? 0 : Math.Round(courseDetails.Average(c => (double)c.progress), 1),
            avgQuizScore = attempts.Count == 0 ? 0 : Math.Round(attempts.Average(a => a.ScorePercent), 1),
            courses = courseDetails,
            recentAttempts = attempts.OrderByDescending(a => a.AttemptedAtUtc).Take(10).Select(a => new
            {
                a.Id,
                a.ScorePercent,
                a.Passed,
                a.AttemptedAtUtc
            })
        });
    }

    public record InviteAgentDto(string Email, string FullName, string Password);

    [HttpPost("agents/invite")]
    public async Task<IActionResult> InviteAgent([FromBody] InviteAgentDto dto)
    {
        var existing = await userManager.FindByEmailAsync(dto.Email);
        if (existing is not null) return Conflict(new { message = "An agent with this email already exists." });

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await userManager.AddToRoleAsync(user, "Agent");
        return Ok(new { id = user.Id, email = user.Email, fullName = user.FullName });
    }

    // ─────────────────────── GROUPS ───────────────────────

    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups()
    {
        var groups = await db.AgentGroups.Include(g => g.Memberships).ToListAsync();
        return Ok(groups.Select(g => new
        {
            id = g.Id,
            name = g.Name,
            memberCount = g.Memberships.Count
        }));
    }

    public record GroupDto(string Name, List<string> AgentIds);

    [HttpPost("groups")]
    public async Task<IActionResult> CreateGroup([FromBody] GroupDto dto)
    {
        var group = new AgentGroup { Name = dto.Name };
        foreach (var agentId in dto.AgentIds.Distinct())
            group.Memberships.Add(new GroupMembership { AgentId = agentId });
        db.AgentGroups.Add(group);
        await db.SaveChangesAsync();
        return Ok(new { id = group.Id });
    }

    // ─────────────────────── ASSIGNMENTS ───────────────────────

    public record AssignDto(int CourseId, List<string> AgentIds, List<int> GroupIds);

    [HttpPost("assignments")]
    public async Task<IActionResult> AssignCourse([FromBody] AssignDto dto)
    {
        foreach (var agentId in dto.AgentIds.Distinct())
        {
            if (!await db.CourseAssignments.AnyAsync(a => a.CourseId == dto.CourseId && a.AgentId == agentId))
                db.CourseAssignments.Add(new CourseAssignment { CourseId = dto.CourseId, AgentId = agentId });
        }
        foreach (var groupId in dto.GroupIds.Distinct())
        {
            if (!await db.GroupCourseAssignments.AnyAsync(a => a.CourseId == dto.CourseId && a.AgentGroupId == groupId))
                db.GroupCourseAssignments.Add(new GroupCourseAssignment { CourseId = dto.CourseId, AgentGroupId = groupId });
        }
        await db.SaveChangesAsync();
        return Ok();
    }
}
