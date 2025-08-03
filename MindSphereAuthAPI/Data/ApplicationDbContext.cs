using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MindSphereAuthAPI.Models;

namespace MindSphereAuthAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Counsellor> Counsellors { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        // Quiz related DbSets
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
        public DbSet<AssignedQuiz> AssignedQuizzes { get; set; }
        public DbSet<QuizResponse> QuizResponses { get; set; }

        

        

         
    }
}
