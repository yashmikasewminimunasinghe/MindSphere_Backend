using MindSphereAuthAPI.Models;

public record QuizDto(
    int Id,
    string Title,
    IEnumerable<QuestionDto> Questions);

public record QuestionDto(
    int Id,
    string Text,
    QuestionType Type,
    IEnumerable<OptionDto>? Options);

public record OptionDto(int Id, string Text, int Score);

public record AssignQuizRequest(int QuizId, int BookingId);
public record SubmitAnswerRequest(int QuestionId, int? OptionId, int? ScaleAnswer);
