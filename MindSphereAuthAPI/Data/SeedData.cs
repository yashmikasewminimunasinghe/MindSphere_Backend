using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MindSphereAuthAPI.Data;
using MindSphereAuthAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MindSphereAuthAPI.Data
{
    public static class SeedData
    {
        public static async Task SeedQuizzes(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (await db.Quizzes.AnyAsync()) return; // If any quiz exists, skip seeding

            var phq9 = new Quiz { Title = "PHQ 9 Depression Questionnaire" };
            var texts = new[]
            {
                "Little interest or pleasure in doing things",
                "Feeling down, depressed, or hopeless",
                "Trouble falling or staying asleep, or sleeping too much",
                "Feeling tired or having little energy",
                "Poor appetite or overeating",
                "Feeling bad about yourself — or that you are a failure",
                "Trouble concentrating on things",
                "Moving or speaking so slowly…",
                "Thoughts that you would be better off dead"
            };

            foreach (var t in texts)
            {
                var q = new QuizQuestion { Text = t, Type = QuestionType.Scale0To3 };
                for (int i = 0; i < 4; i++)
                {
                    q.Options.Add(new QuizOption { Text = i.ToString(), Score = i });
                }
                phq9.Questions.Add(q);
            }

            db.Quizzes.Add(phq9);
            await db.SaveChangesAsync();
        }
    }
}
