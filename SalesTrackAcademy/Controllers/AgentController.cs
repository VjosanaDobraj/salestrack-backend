using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesTrackAcademy.Data;
using SalesTrackAcademy.Models;
using SalesTrackAcademy.Models.ViewModels;

namespace SalesTrackAcademy.Controllers;

[Authorize(Roles = "Agent,Admin")]
public class AgentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var assignedCourseIds = await GetAssignedCourseIdsAsync(user.Id);

        var courses = await context.Courses
            .Where(x => assignedCourseIds.Contains(x.Id))
            .Include(x => x.Lessons)
            .OrderBy(x => x.Title)
            .ToListAsync();

        var progressMap = await context.LessonProgressRecords
            .Where(x => x.AgentId == user.Id)
            .ToDictionaryAsync(x => x.LessonId, x => x.IsCompleted);

        var model = new AgentDashboardVm
        {
            AssignedCourses = courses.Select(course =>
            {
                var lessonIds = course.Lessons.Select(x => x.Id).ToList();
                var completed = lessonIds.Count(id => progressMap.TryGetValue(id, out var done) && done);
                var percent = lessonIds.Count == 0 ? 0 : (int)Math.Round((double)completed / lessonIds.Count * 100);

                return new AssignedCourseVm
                {
                    CourseId = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    ThumbnailUrl = course.ThumbnailUrl,
                    ProgressPercent = percent,
                    IsCompleted = percent == 100
                };
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Course(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var assignedCourseIds = await GetAssignedCourseIdsAsync(user.Id);
        if (!assignedCourseIds.Contains(id))
        {
            return Forbid();
        }

        var course = await context.Courses
            .Include(x => x.Lessons)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (course is null)
        {
            return NotFound();
        }

        var lessonIds = course.Lessons.Select(x => x.Id).ToList();
        ViewBag.Progress = await context.LessonProgressRecords
            .Where(x => x.AgentId == user.Id && lessonIds.Contains(x.LessonId))
            .ToDictionaryAsync(x => x.LessonId, x => x.IsCompleted);

        return View(course);
    }

    [HttpGet]
    public async Task<IActionResult> Lesson(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var lesson = await context.Lessons
            .Include(x => x.Course)
            .Include(x => x.QuizQuestions)
                .ThenInclude(x => x.Options)
            .Include(x => x.Comments)
                .ThenInclude(x => x.Agent)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (lesson is null)
        {
            return NotFound();
        }

        var assignedCourseIds = await GetAssignedCourseIdsAsync(user.Id);
        if (!assignedCourseIds.Contains(lesson.CourseId))
        {
            return Forbid();
        }

        var progress = await context.LessonProgressRecords
            .FirstOrDefaultAsync(x => x.LessonId == id && x.AgentId == user.Id);

        var model = new LessonPlayerVm
        {
            CourseId = lesson.CourseId,
            LessonId = lesson.Id,
            CourseTitle = lesson.Course?.Title ?? string.Empty,
            LessonTitle = lesson.Title,
            LessonType = lesson.LessonType,
            ContentUrl = lesson.ContentUrl,
            TextContent = lesson.TextContent,
            PassingScorePercent = lesson.PassingScorePercent,
            IsCompleted = progress?.IsCompleted ?? false,
            QuizQuestions = lesson.QuizQuestions,
            Comments = lesson.Comments.OrderByDescending(x => x.CreatedAtUtc).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz(int lessonId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var lesson = await context.Lessons
            .Include(x => x.QuizQuestions)
                .ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.Id == lessonId);

        if (lesson is null)
        {
            return NotFound();
        }

        var total = lesson.QuizQuestions.Count;
        if (total == 0)
        {
            return RedirectToAction(nameof(Lesson), new { id = lessonId });
        }

        var correct = 0;
        foreach (var question in lesson.QuizQuestions)
        {
            var selectedOptionId = Request.Form[$"answer_{question.Id}"].ToString();
            if (int.TryParse(selectedOptionId, out var optionId) && question.Options.Any(x => x.Id == optionId && x.IsCorrect))
            {
                correct++;
            }
        }

        var score = (int)Math.Round((double)correct / total * 100);

        context.QuizAttempts.Add(new QuizAttempt
        {
            AgentId = user.Id,
            LessonId = lessonId,
            ScorePercent = score
        });

        var progress = await context.LessonProgressRecords
            .FirstOrDefaultAsync(x => x.LessonId == lessonId && x.AgentId == user.Id);

        if (progress is null)
        {
            progress = new LessonProgress
            {
                LessonId = lessonId,
                AgentId = user.Id,
                TimeSpentMinutes = 10
            };
            context.LessonProgressRecords.Add(progress);
        }

        var passing = lesson.PassingScorePercent ?? 0;
        if (score >= passing)
        {
            progress.IsCompleted = true;
            progress.CompletedAtUtc = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        TempData["QuizResult"] = $"You scored {score}%.";

        return RedirectToAction(nameof(Lesson), new { id = lessonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteLesson(int lessonId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var lesson = await context.Lessons.FindAsync(lessonId);
        if (lesson is null)
        {
            return NotFound();
        }

        var progress = await context.LessonProgressRecords
            .FirstOrDefaultAsync(x => x.LessonId == lessonId && x.AgentId == user.Id);

        if (progress is null)
        {
            progress = new LessonProgress
            {
                LessonId = lessonId,
                AgentId = user.Id,
                TimeSpentMinutes = 8
            };
            context.LessonProgressRecords.Add(progress);
        }

        progress.IsCompleted = true;
        progress.CompletedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Lesson), new { id = lessonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(CommentVm vm)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Lesson), new { id = vm.LessonId });
        }

        context.LessonComments.Add(new LessonComment
        {
            LessonId = vm.LessonId,
            AgentId = user.Id,
            Body = vm.Body
        });

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Lesson), new { id = vm.LessonId });
    }

    private async Task<List<int>> GetAssignedCourseIdsAsync(string agentId)
    {
        var directIds = await context.CourseAssignments
            .Where(x => x.AgentId == agentId)
            .Select(x => x.CourseId)
            .ToListAsync();

        var groupIds = await context.GroupMemberships
            .Where(x => x.AgentId == agentId)
            .Select(x => x.AgentGroupId)
            .ToListAsync();

        var groupCourseIds = await context.GroupCourseAssignments
            .Where(x => groupIds.Contains(x.AgentGroupId))
            .Select(x => x.CourseId)
            .ToListAsync();

        return directIds.Union(groupCourseIds).Distinct().ToList();
    }
}
