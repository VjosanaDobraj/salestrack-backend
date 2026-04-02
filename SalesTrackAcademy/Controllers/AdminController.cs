using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesTrackAcademy.Data;
using SalesTrackAcademy.Models;
using SalesTrackAcademy.Models.ViewModels;

namespace SalesTrackAcademy.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var agents = await userManager.GetUsersInRoleAsync("Agent");
        var courses = await context.Courses.Include(x => x.Lessons).ToListAsync();
        var progress = await context.LessonProgressRecords.Where(x => x.IsCompleted).ToListAsync();
        var attempts = await context.QuizAttempts.ToListAsync();
        var comments = await context.LessonComments.CountAsync();

        var totalLessons = courses.Sum(c => c.Lessons.Count);
        var model = new AdminDashboardVm
        {
            TotalCourses = courses.Count,
            TotalAgents = agents.Count,
            AvgCompletionRate = totalLessons == 0 || agents.Count == 0
                ? 0
                : Math.Round((double)progress.Count / (totalLessons * agents.Count) * 100, 1),
            AvgQuizScore = attempts.Count == 0 ? 0 : Math.Round(attempts.Average(x => x.ScorePercent), 1),
            TotalComments = comments,
            AgentProgress = await BuildAgentProgressAsync(agents)
        };

        return View(model);
    }

    public async Task<IActionResult> Courses()
    {
        var courses = await context.Courses
            .Include(x => x.Lessons.OrderBy(l => l.SortOrder))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return View(courses);
    }

    [HttpGet]
    public IActionResult CreateCourse()
    {
        return View(new CourseEditVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CourseEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var admin = await userManager.GetUserAsync(User);
        if (admin is null)
        {
            return Challenge();
        }

        var course = new Course
        {
            Title = vm.Title,
            Description = vm.Description,
            ThumbnailUrl = vm.ThumbnailUrl,
            CreatedById = admin.Id
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(CourseBuilder), new { id = course.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id)
    {
        var course = await context.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        return View(new CourseEditVm
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            ThumbnailUrl = course.ThumbnailUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(CourseEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var course = await context.Courses.FindAsync(vm.Id);
        if (course is null)
        {
            return NotFound();
        }

        course.Title = vm.Title;
        course.Description = vm.Description;
        course.ThumbnailUrl = vm.ThumbnailUrl;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(CourseBuilder), new { id = vm.Id });
    }

    [HttpGet]
    public async Task<IActionResult> CourseBuilder(int id)
    {
        var course = await context.Courses
            .Include(x => x.Lessons.OrderBy(l => l.SortOrder))
                .ThenInclude(l => l.QuizQuestions)
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (course is null)
        {
            return NotFound();
        }

        ViewBag.Agents = await userManager.GetUsersInRoleAsync("Agent");
        ViewBag.Groups = await context.AgentGroups.Include(x => x.Memberships).ToListAsync();

        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLesson(LessonEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(CourseBuilder), new { id = vm.CourseId });
        }

        var sortOrder = await context.Lessons
            .Where(x => x.CourseId == vm.CourseId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync() ?? 0;

        var lesson = new Lesson
        {
            CourseId = vm.CourseId,
            Title = vm.Title,
            LessonType = vm.LessonType,
            ContentUrl = vm.ContentUrl,
            TextContent = vm.TextContent,
            PassingScorePercent = vm.PassingScorePercent,
            SortOrder = sortOrder + 1
        };

        context.Lessons.Add(lesson);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(CourseBuilder), new { id = vm.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveLesson(int lessonId, bool moveUp)
    {
        var lesson = await context.Lessons.FindAsync(lessonId);
        if (lesson is null)
        {
            return NotFound();
        }

        var lessons = await context.Lessons
            .Where(x => x.CourseId == lesson.CourseId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        var index = lessons.FindIndex(x => x.Id == lessonId);
        if (index < 0)
        {
            return NotFound();
        }

        var swapIndex = moveUp ? index - 1 : index + 1;
        if (swapIndex < 0 || swapIndex >= lessons.Count)
        {
            return RedirectToAction(nameof(CourseBuilder), new { id = lesson.CourseId });
        }

        var current = lessons[index];
        var swap = lessons[swapIndex];
        (current.SortOrder, swap.SortOrder) = (swap.SortOrder, current.SortOrder);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(CourseBuilder), new { id = lesson.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReorderLessons(int courseId, string orderedLessonIds)
    {
        var parsedLessonIds = orderedLessonIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id, out var parsed) ? parsed : 0)
            .Where(id => id > 0)
            .ToList();

        var lessons = await context.Lessons
            .Where(x => x.CourseId == courseId)
            .ToListAsync();

        var orderMap = parsedLessonIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index + 1);

        foreach (var lesson in lessons)
        {
            if (orderMap.TryGetValue(lesson.Id, out var order))
            {
                lesson.SortOrder = order;
            }
        }

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(CourseBuilder), new { id = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuizQuestion(QuizQuestionVm vm)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(CourseBuilder), new { id = await GetCourseIdForLessonAsync(vm.LessonId) });
        }

        var question = new QuizQuestion
        {
            LessonId = vm.LessonId,
            Prompt = vm.Prompt,
            Options =
            [
                new QuizOption { OptionText = vm.OptionA, IsCorrect = vm.CorrectOption == 1 },
                new QuizOption { OptionText = vm.OptionB, IsCorrect = vm.CorrectOption == 2 },
                new QuizOption { OptionText = vm.OptionC, IsCorrect = vm.CorrectOption == 3 },
                new QuizOption { OptionText = vm.OptionD, IsCorrect = vm.CorrectOption == 4 }
            ]
        };

        context.QuizQuestions.Add(question);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(CourseBuilder), new { id = await GetCourseIdForLessonAsync(vm.LessonId) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignCourse(AssignmentVm vm)
    {
        var courseExists = await context.Courses.AnyAsync(x => x.Id == vm.CourseId);
        if (!courseExists)
        {
            return NotFound();
        }

        foreach (var agentId in vm.AgentIds.Distinct())
        {
            var exists = await context.CourseAssignments.AnyAsync(x => x.CourseId == vm.CourseId && x.AgentId == agentId);
            if (!exists)
            {
                context.CourseAssignments.Add(new CourseAssignment
                {
                    CourseId = vm.CourseId,
                    AgentId = agentId
                });
            }
        }

        foreach (var groupId in vm.GroupIds.Distinct())
        {
            var exists = await context.GroupCourseAssignments.AnyAsync(x => x.CourseId == vm.CourseId && x.AgentGroupId == groupId);
            if (!exists)
            {
                context.GroupCourseAssignments.Add(new GroupCourseAssignment
                {
                    CourseId = vm.CourseId,
                    AgentGroupId = groupId
                });
            }
        }

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(CourseBuilder), new { id = vm.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGroup(GroupCreateVm vm)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Courses));
        }

        var group = new AgentGroup { Name = vm.Name };
        foreach (var agentId in vm.AgentIds.Distinct())
        {
            group.Memberships.Add(new GroupMembership { AgentId = agentId });
        }

        context.AgentGroups.Add(group);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Courses));
    }

    [HttpGet]
    public async Task<IActionResult> AgentDetail(string id)
    {
        var agent = await userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (agent is null)
        {
            return NotFound();
        }

        var detail = (await BuildAgentProgressAsync([agent])).First();
        return View(detail);
    }

    private async Task<List<AgentProgressVm>> BuildAgentProgressAsync(IList<ApplicationUser> agents)
    {
        var results = new List<AgentProgressVm>();

        foreach (var agent in agents)
        {
            var directCourseIds = await context.CourseAssignments
                .Where(x => x.AgentId == agent.Id)
                .Select(x => x.CourseId)
                .ToListAsync();

            var groupIds = await context.GroupMemberships
                .Where(x => x.AgentId == agent.Id)
                .Select(x => x.AgentGroupId)
                .ToListAsync();

            var groupCourseIds = await context.GroupCourseAssignments
                .Where(x => groupIds.Contains(x.AgentGroupId))
                .Select(x => x.CourseId)
                .ToListAsync();

            var assignedCourseIds = directCourseIds.Union(groupCourseIds).Distinct().ToList();
            var assignedLessons = await context.Lessons
                .Where(x => assignedCourseIds.Contains(x.CourseId))
                .Select(x => x.Id)
                .ToListAsync();

            var completedLessonIds = await context.LessonProgressRecords
                .Where(x => x.AgentId == agent.Id && x.IsCompleted && assignedLessons.Contains(x.LessonId))
                .Select(x => x.LessonId)
                .ToListAsync();

            var completedLessonCount = completedLessonIds.Count;

            var totalLessonCount = assignedLessons.Count;

            var lessonsByCourse = await context.Lessons
                .Where(x => assignedCourseIds.Contains(x.CourseId))
                .GroupBy(x => x.CourseId)
                .Select(g => new { CourseId = g.Key, LessonIds = g.Select(l => l.Id).ToList() })
                .ToListAsync();

            var completedCourseCount = lessonsByCourse.Count(x =>
                x.LessonIds.Count > 0 && x.LessonIds.All(id => completedLessonIds.Contains(id)));

            var bestQuizScore = await context.QuizAttempts
                .Where(x => x.AgentId == agent.Id)
                .Select(x => (int?)x.ScorePercent)
                .MaxAsync() ?? 0;

            results.Add(new AgentProgressVm
            {
                AgentName = string.IsNullOrWhiteSpace(agent.FullName) ? agent.UserName ?? "Agent" : agent.FullName,
                AgentEmail = agent.Email ?? string.Empty,
                AssignedCourses = assignedCourseIds.Count,
                CompletedCourses = completedCourseCount,
                CompletionPercent = totalLessonCount == 0 ? 0 : (int)Math.Round((double)completedLessonCount / totalLessonCount * 100),
                BestQuizScore = bestQuizScore
            });
        }

        return results.OrderByDescending(x => x.CompletionPercent).ThenBy(x => x.AgentName).ToList();
    }

    private async Task<int> GetCourseIdForLessonAsync(int lessonId)
    {
        var courseId = await context.Lessons
            .Where(x => x.Id == lessonId)
            .Select(x => x.CourseId)
            .FirstOrDefaultAsync();

        return courseId;
    }
}
