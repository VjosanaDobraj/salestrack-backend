using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesTrackAcademy.Data;
using SalesTrackAcademy.Models;

namespace SalesTrackAcademy.Controllers.Api;

[ApiController]
[Route("api/agent")]
[Authorize(Roles = "Agent,Admin")]
public class AgentApiController(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    private async Task<List<int>> GetAssignedCourseIdsAsync(string userId)
    {
        var direct = await db.CourseAssignments.Where(a => a.AgentId == userId).Select(a => a.CourseId).ToListAsync();
        var groupIds = await db.GroupMemberships.Where(m => m.AgentId == userId).Select(m => m.AgentGroupId).ToListAsync();
        var groupCourseIds = await db.GroupCourseAssignments.Where(a => groupIds.Contains(a.AgentGroupId)).Select(a => a.CourseId).ToListAsync();
        return direct.Union(groupCourseIds).Distinct().ToList();
    }

    // ─────────────────────── MY COURSES ───────────────────────

    [HttpGet("courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var assignedIds = await GetAssignedCourseIdsAsync(user.Id);
        var courses = await db.Courses
            .Include(c => c.Lessons)
            .Where(c => assignedIds.Contains(c.Id))
            .OrderBy(c => c.Title)
            .ToListAsync();

        var progressMap = await db.LessonProgressRecords
            .Where(p => p.AgentId == user.Id)
            .ToDictionaryAsync(p => p.LessonId, p => p.IsCompleted);

        var result = courses.Select(course =>
        {
            var lessonIds = course.Lessons.Select(l => l.Id).ToList();
            var completed = lessonIds.Count(id => progressMap.TryGetValue(id, out var done) && done);
            var pct = lessonIds.Count == 0 ? 0 : (int)Math.Round((double)completed / lessonIds.Count * 100);
            return new
            {
                id = course.Id,
                title = course.Title,
                description = course.Description,
                thumbnailUrl = course.ThumbnailUrl,
                progressPercent = pct,
                isCompleted = pct == 100,
                lessonCount = lessonIds.Count,
                completedLessons = completed
            };
        });

        return Ok(result);
    }

    [HttpGet("courses/{id}")]
    public async Task<IActionResult> GetCourse(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var assignedIds = await GetAssignedCourseIdsAsync(user.Id);
        if (!assignedIds.Contains(id)) return Forbid();

        var course = await db.Courses
            .Include(c => c.Lessons.OrderBy(l => l.SortOrder))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course is null) return NotFound();

        var progressMap = await db.LessonProgressRecords
            .Where(p => p.AgentId == user.Id)
            .ToDictionaryAsync(p => p.LessonId, p => p.IsCompleted);

        return Ok(new
        {
            id = course.Id,
            title = course.Title,
            description = course.Description,
            thumbnailUrl = course.ThumbnailUrl,
            lessons = course.Lessons.Select(l => new
            {
                id = l.Id,
                title = l.Title,
                lessonType = l.LessonType.ToString(),
                sortOrder = l.SortOrder,
                isCompleted = progressMap.TryGetValue(l.Id, out var done) && done
            })
        });
    }

    // ─────────────────────── LESSON PLAYER ───────────────────────

    [HttpGet("lessons/{id}")]
    public async Task<IActionResult> GetLesson(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var lesson = await db.Lessons
            .Include(l => l.QuizQuestions.OrderBy(q => q.Id))
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson is null) return NotFound();

        var assignedIds = await GetAssignedCourseIdsAsync(user.Id);
        if (!assignedIds.Contains(lesson.CourseId)) return Forbid();

        var isCompleted = await db.LessonProgressRecords.AnyAsync(p => p.AgentId == user.Id && p.LessonId == id && p.IsCompleted);
        var lastAttempt = await db.QuizAttempts.Where(a => a.AgentId == user.Id && a.LessonId == id).OrderByDescending(a => a.AttemptedAtUtc).FirstOrDefaultAsync();

        var comments = await db.LessonComments
            .Where(c => c.LessonId == id)
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync();

        // Get author names
        var authorIds = comments.Select(c => c.AgentId).Distinct().ToList();
        var authors = await db.Users.Where(u => authorIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.UserName ?? u.Email ?? "User");

        // Quiz questions — hide IsCorrect from agent
        var quizQuestions = lesson.QuizQuestions.Select(q => new
        {
            id = q.Id,
            prompt = q.Prompt,
            options = q.Options.Select(o => new { id = o.Id, text = o.OptionText })
        });

        return Ok(new
        {
            id = lesson.Id,
            courseId = lesson.CourseId,
            title = lesson.Title,
            lessonType = lesson.LessonType.ToString(),
            contentUrl = lesson.ContentUrl,
            textContent = lesson.TextContent,
            passingScorePercent = lesson.PassingScorePercent,
            isCompleted,
            lastAttempt = lastAttempt is null ? null : new { lastAttempt.ScorePercent, lastAttempt.Passed, lastAttempt.AttemptedAtUtc },
            quizQuestions,
            comments = comments.Select(c => new
            {
                id = c.Id,
                body = c.Body,
                authorName = authors.TryGetValue(c.AgentId, out var name) ? name : "User",
                postedAt = c.CreatedAtUtc
            })
        });
    }

    [HttpPost("lessons/{id}/complete")]
    public async Task<IActionResult> MarkComplete(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var existing = await db.LessonProgressRecords.FirstOrDefaultAsync(p => p.AgentId == user.Id && p.LessonId == id);
        if (existing is null)
        {
            db.LessonProgressRecords.Add(new LessonProgress { AgentId = user.Id, LessonId = id, IsCompleted = true });
        }
        else
        {
            existing.IsCompleted = true;
        }

        await db.SaveChangesAsync();
        return Ok();
    }

    // ─────────────────────── QUIZ ───────────────────────

    public record QuizSubmitDto(Dictionary<int, int> Answers); // questionId -> optionId

    [HttpPost("lessons/{id}/quiz/submit")]
    public async Task<IActionResult> SubmitQuiz(int id, [FromBody] QuizSubmitDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var lesson = await db.Lessons
            .Include(l => l.QuizQuestions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson is null) return NotFound();

        int correct = 0;
        foreach (var (questionId, selectedOptionId) in dto.Answers)
        {
            var question = lesson.QuizQuestions.FirstOrDefault(q => q.Id == questionId);
            if (question is null) continue;
            var selectedOption = question.Options.FirstOrDefault(o => o.Id == selectedOptionId);
            if (selectedOption?.IsCorrect == true) correct++;
        }

        int total = lesson.QuizQuestions.Count;
        int scorePercent = total == 0 ? 0 : (int)Math.Round((double)correct / total * 100);
        bool passed = scorePercent >= lesson.PassingScorePercent;

        db.QuizAttempts.Add(new QuizAttempt
        {
            AgentId = user.Id,
            LessonId = id,
            ScorePercent = scorePercent,
            Passed = passed,
            AttemptedAtUtc = DateTime.UtcNow
        });

        if (passed)
        {
            var progress = await db.LessonProgressRecords.FirstOrDefaultAsync(p => p.AgentId == user.Id && p.LessonId == id);
            if (progress is null)
                db.LessonProgressRecords.Add(new LessonProgress { AgentId = user.Id, LessonId = id, IsCompleted = true });
            else
                progress.IsCompleted = true;
        }

        await db.SaveChangesAsync();
        return Ok(new { scorePercent, correct, total, passed, passingScore = lesson.PassingScorePercent });
    }

    // ─────────────────────── COMMENTS ───────────────────────

    public record CommentDto(string Body);

    [HttpPost("lessons/{id}/comments")]
    public async Task<IActionResult> PostComment(int id, [FromBody] CommentDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Body)) return BadRequest("Comment cannot be empty.");

        var comment = new LessonComment
        {
            LessonId = id,
            AgentId = user.Id,
            Body = dto.Body.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        db.LessonComments.Add(comment);
        await db.SaveChangesAsync();

        return Ok(new
        {
            id = comment.Id,
            body = comment.Body,
            authorName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName,
            postedAt = comment.CreatedAtUtc
        });
    }
}
