using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalesTrackAcademy.Models;

namespace SalesTrackAcademy.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        var roles = new[] { "Admin", "Agent" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = await EnsureUser(userManager, "admin@salestrack.local", "Admin Manager", "Temp123!", "Admin");
        var agentOne = await EnsureUser(userManager, "agent.a@salestrack.local", "Agent A", "Temp123!", "Agent");
        var agentTwo = await EnsureUser(userManager, "agent.b@salestrack.local", "Agent B", "Temp123!", "Agent");

        if (!await context.Courses.AnyAsync())
        {
            var course = new Course
            {
                Title = "Cold Calling Techniques",
                Description = "Practical tactics, scripts, and call flow exercises for early-stage outbound reps.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1556740749-887f6717d7e4?auto=format&fit=crop&w=1200&q=80",
                CreatedById = admin.Id,
                CreatedAtUtc = DateTime.UtcNow
            };

            var lessonVideo = new Lesson
            {
                Title = "Opening Hooks That Win Attention",
                LessonType = LessonType.Video,
                ContentUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                SortOrder = 1,
                PassingScorePercent = 80
            };

            var lessonPdf = new Lesson
            {
                Title = "High-Conversion Cold Call Script",
                LessonType = LessonType.Pdf,
                ContentUrl = "https://mozilla.github.io/pdf.js/web/compressed.tracemonkey-pldi-09.pdf",
                SortOrder = 2
            };

            var lessonAudio = new Lesson
            {
                Title = "Top Performer Call Breakdown",
                LessonType = LessonType.Audio,
                ContentUrl = "https://file-examples.com/storage/fe0f4f2dd3fbe2a1f6f1f3f/2017/11/file_example_MP3_700KB.mp3",
                SortOrder = 3
            };

            var lessonText = new Lesson
            {
                Title = "Objection Handling Patterns",
                LessonType = LessonType.Text,
                TextContent = "When price objections appear, ask discovery questions first, anchor to business outcomes, then reconfirm urgency before offering options.",
                SortOrder = 4,
                PassingScorePercent = 80
            };

            var question = new QuizQuestion
            {
                Prompt = "Which opening increases the chance of continuing the call?",
                Options =
                [
                    new QuizOption { OptionText = "Do you have five minutes?", IsCorrect = false },
                    new QuizOption { OptionText = "I know you were not expecting my call; may I share one idea that helped teams like yours?", IsCorrect = true },
                    new QuizOption { OptionText = "I can offer a discount today only.", IsCorrect = false },
                    new QuizOption { OptionText = "Can you connect me to your manager?", IsCorrect = false }
                ]
            };

            lessonText.QuizQuestions.Add(question);

            course.Lessons.Add(lessonVideo);
            course.Lessons.Add(lessonPdf);
            course.Lessons.Add(lessonAudio);
            course.Lessons.Add(lessonText);

            context.Courses.Add(course);

            var group = new AgentGroup { Name = "New Hires" };
            group.Memberships.Add(new GroupMembership { AgentId = agentOne.Id });
            group.Memberships.Add(new GroupMembership { AgentId = agentTwo.Id });

            context.AgentGroups.Add(group);

            await context.SaveChangesAsync();

            context.GroupCourseAssignments.Add(new GroupCourseAssignment
            {
                AgentGroupId = group.Id,
                CourseId = course.Id
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUser(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unable to create seeded user '{email}'.");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }
}
