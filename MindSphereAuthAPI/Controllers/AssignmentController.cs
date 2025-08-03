using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindSphereAuthAPI.Data;
using MindSphereAuthAPI.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class AssignmentController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public AssignmentController(ApplicationDbContext db) => _db = db;

    // COUNSELLOR assigns quiz to booking
    [HttpPost("assign")]
    [Authorize(Roles = "Counselor")]
    public async Task<IActionResult> Assign(AssignQuizRequest req)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == req.BookingId);
        if (booking is null) return NotFound("Booking not found");
        var assigned = new AssignedQuiz { QuizId = req.QuizId, BookingId = req.BookingId };
        _db.AssignedQuizzes.Add(assigned);
        await _db.SaveChangesAsync();
        return Ok(assigned.Id);
    }

    // CLIENT – fetch quizzes waiting for them
    [HttpGet("pending")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Pending()
    {
        var userId = User.FindFirstValue("id");
        if (userId is null)
            return Unauthorized("User id claim missing in token");

        var pending = await _db.AssignedQuizzes
            .Include(a => a.Booking)
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Options)
            .Where(a => a.Booking.ClientId == userId && !a.Completed)
            .Select(a => new
            {
                id = a.Id,
                assignedAt = a.AssignedAt,
                title = a.Quiz.Title,
                questions = a.Quiz.Questions.Select(q => new
                {
                    id = q.Id,
                    text = q.Text,
                    type = q.Type,
                    options = q.Options.Select(o => new { o.Id, o.Text })
                })
            })
            .ToListAsync();

        return Ok(pending);
    }

    // CLIENT – submit answers
    [HttpPost("{assignedId}/submit")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Submit(int assignedId, [FromBody] IEnumerable<SubmitAnswerRequest> answers)
    {
        var assigned = await _db.AssignedQuizzes
            .Include(a => a.Quiz).ThenInclude(q => q.Questions).ThenInclude(qq => qq.Options)
            .FirstOrDefaultAsync(a => a.Id == assignedId);

        if (assigned is null || assigned.Completed) return BadRequest();
        int total = 0;

        foreach (var ans in answers)
        {
            var question = assigned.Quiz.Questions.First(q => q.Id == ans.QuestionId);
            var resp = new QuizResponse
            {
                AssignedQuizId = assignedId,
                QuizQuestionId = ans.QuestionId,
                SelectedOptionId = ans.OptionId,
                ScaleAnswer = ans.ScaleAnswer
            };
            _db.QuizResponses.Add(resp);
            total += question.Type switch
            {
                QuestionType.Scale0To3 => ans.ScaleAnswer ?? 0,
                _ => question.Options.First(o => o.Id == ans.OptionId).Score
            };
        }

        assigned.Completed = true;
        assigned.TotalScore = total;
        await _db.SaveChangesAsync();
        return Ok(total);
    }

    // COUNSELLOR – view results by booking/client (FIXED switch expression)
    [HttpGet("results/{bookingId}")]
    [Authorize(Roles = "Counselor")]
    public async Task<IActionResult> Results(int bookingId)
    {
        var rawResults = await _db.AssignedQuizzes
            .Include(a => a.Quiz)
            .Include(a => a.Booking)
                .ThenInclude(b => b.Client)
            .Include(a => a.Responses)
                .ThenInclude(r => r.QuizQuestion)
            .Where(a => a.BookingId == bookingId && a.Completed)
            .ToListAsync();

        var list = rawResults.Select(a => new
        {
            a.Id,
            a.TotalScore,
            a.Quiz.Title,
            a.AssignedAt,
            ClientName = a.Booking.Client.FirstName + " " + a.Booking.Client.LastName,
            MentalLevel = a.TotalScore <= 4 ? "Minimal Depression"
                        : a.TotalScore <= 9 ? "Mild Depression"
                        : a.TotalScore <= 14 ? "Moderate Depression"
                        : a.TotalScore <= 19 ? "Moderately Severe Depression"
                        : "Severe Depression",
            Answers = a.Responses.Select(r => new
            {
                r.QuizQuestion.Text,
                r.ScaleAnswer,
                r.SelectedOptionId
            })
        }).ToList();

        return Ok(list);
    }
}
