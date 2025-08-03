using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindSphereAuthAPI.Data;
using MindSphereAuthAPI.Dtos;
using MindSphereAuthAPI.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public QuizController(ApplicationDbContext db) => _db = db;

    // 1. Create a quiz (Counsellor or Admin only)
    [HttpPost]
    [Authorize(Roles = "Counselor,Admin")]
    public async Task<IActionResult> Create(QuizDto dto)
    {
        var quiz = new Quiz { Title = dto.Title };

        foreach (var q in dto.Questions)
        {
            var question = new QuizQuestion { Text = q.Text, Type = q.Type };
            if (q.Options != null)
            {
                foreach (var o in q.Options)
                    question.Options.Add(new QuizOption { Text = o.Text, Score = o.Score });
            }
            quiz.Questions.Add(question);
        }

        _db.Quizzes.Add(quiz);
        await _db.SaveChangesAsync();

        return Ok(new { quiz.Id });
    }

    // 2. List all quizzes (Counsellor/Admin only)
    [HttpGet]
    [Authorize(Roles = "Counselor,Admin")]
    public async Task<IEnumerable<QuizDto>> Get()
    {
        return await _db.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(qq => qq.Options)
            .Select(q => new QuizDto(
                q.Id,
                q.Title,
                q.Questions.Select(qq => new QuestionDto(
                    qq.Id,
                    qq.Text,
                    qq.Type,
                    qq.Options.Select(o => new OptionDto(o.Id, o.Text, o.Score)))))
            ).ToListAsync();
    }

    // 3. Assign quiz to a booking (Only if booking belongs to this counsellor)
    [HttpPost("{bookingId}/assign")]
    [Authorize(Roles = "Counselor")]
    public async Task<IActionResult> Assign(int bookingId, [FromBody] int quizId)
    {
        var userId = User.FindFirstValue("id");

        // Find counsellor by logged-in user ID
        var counsellor = await _db.Counsellors.FirstOrDefaultAsync(c => c.UserId == userId);
        if (counsellor == null)
            return NotFound(new { message = "Counsellor not found for current user." });

        // Confirm that the booking is owned by this counsellor
        var booking = await _db.Bookings
            .Include(b => b.Client)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.CounsellorId == counsellor.Id);

        if (booking == null)
            return Forbid("This booking does not belong to you.");

        // Check if quiz exists
        var quizExists = await _db.Quizzes.AnyAsync(q => q.Id == quizId);
        if (!quizExists)
            return NotFound(new { message = "Quiz not found." });

        // Prevent duplicate assignment
        bool alreadyAssigned = await _db.AssignedQuizzes
            .AnyAsync(a => a.BookingId == bookingId && a.QuizId == quizId);

        if (alreadyAssigned)
            return BadRequest(new { message = "This quiz is already assigned for this booking." });

        // Assign the quiz
        var assignment = new AssignedQuiz
        {
            BookingId = bookingId,
            QuizId = quizId,
            AssignedAt = DateTime.UtcNow,
            Completed = false
        };

        _db.AssignedQuizzes.Add(assignment);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Quiz assigned successfully!" });
    }
}
