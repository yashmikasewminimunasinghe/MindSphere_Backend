using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindSphereAuthAPI.Models;

public enum QuestionType { Scale0To3 = 0, MultipleChoice = 1 }

public class Quiz
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = default!;
    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
}

public class QuizQuestion
{
    [Key] public int Id { get; set; }
    [ForeignKey(nameof(Quiz))] public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = default!;
    [Required] public string Text { get; set; } = default!;
    public QuestionType Type { get; set; } = QuestionType.Scale0To3;
    public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();

    // Add this collection for responses
    public ICollection<QuizResponse> Responses { get; set; } = new List<QuizResponse>();
}

public class QuizOption
{
    [Key] public int Id { get; set; }
    [ForeignKey(nameof(QuizQuestion))] public int QuizQuestionId { get; set; }
    public QuizQuestion QuizQuestion { get; set; } = default!;
    public string Text { get; set; } = default!;
    public int Score { get; set; }  // scale‑value or MCQ score
}

public class AssignedQuiz
{
    [Key] public int Id { get; set; }
    [ForeignKey(nameof(Quiz))] public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = default!;
    // Link to an existing Booking record
    [ForeignKey(nameof(Booking))] public int BookingId { get; set; }
    public Booking Booking { get; set; } = default!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool Completed { get; set; } = false;
    public int? TotalScore { get; set; }      // convenience
    public ICollection<QuizResponse> Responses { get; set; } = new List<QuizResponse>();
}

public class QuizResponse
{
    [Key] public int Id { get; set; }
    [ForeignKey(nameof(AssignedQuiz))] public int AssignedQuizId { get; set; }
    public AssignedQuiz AssignedQuiz { get; set; } = default!;
    [ForeignKey(nameof(QuizQuestion))] public int QuizQuestionId { get; set; }
    public QuizQuestion QuizQuestion { get; set; } = default!;
    public int? SelectedOptionId { get; set; } // for MCQ / scale (0‑3)
    public int? ScaleAnswer { get; set; }
}
