using Microsoft.EntityFrameworkCore;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;
using ZakirPro.Features.Students.Exams.SubmitAttempt;

namespace ZakirPro.Common.BackgroundServices;

/// <summary>
/// Periodically scans for InProgress exam attempts whose DeadlineAt has passed
/// by a grace period and auto-submits them server-side. This handles students
/// who disconnected or closed the browser without explicitly submitting.
/// </summary>
public class ExamAutoSubmitService : BackgroundService
{
    private static readonly TimeSpan GracePeriod  = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExamAutoSubmitService> _logger;

    public ExamAutoSubmitService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExamAutoSubmitService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExamAutoSubmitService started. Sweep interval: {Interval}.", SweepInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SweepExpiredAttemptsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExamAutoSubmitService encountered an error during sweep.");
            }

            await Task.Delay(SweepInterval, stoppingToken);
        }
    }

    private async Task SweepExpiredAttemptsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cutoff = DateTime.UtcNow - GracePeriod;

        // Find InProgress attempts whose hard deadline has passed the grace period
        var expiredAttempts = await db.StudentExamAttempts
            .Include(a => a.Exam)
            .Include(a => a.Answers)
                .ThenInclude(sa => sa.Question)
            .Include(a => a.Answers)
                .ThenInclude(sa => sa.SelectedChoice)
            .Where(a =>
                a.AttemptStatus == AttemptStatus.InProgress &&
                a.DeadlineAt < cutoff)
            .ToListAsync(ct);

        if (expiredAttempts.Count == 0)
            return;

        _logger.LogInformation(
            "ExamAutoSubmitService: found {Count} expired attempt(s) to auto-submit.", expiredAttempts.Count);

        var now = DateTime.UtcNow;

        foreach (var attempt in expiredAttempts)
        {
            attempt.SubmittedAt      = now;
            attempt.IsAutoSubmitted  = true;
            attempt.TimeTakenSeconds = (int)(now - attempt.StartedAt).TotalSeconds;

            // Auto-grade choice questions
            int autoGradedScore  = 0;
            bool hasEssayQuestions = false;

            foreach (var answer in attempt.Answers)
            {
                if (answer.Question.Type == QuestionType.Essay)
                {
                    hasEssayQuestions = true;
                    continue; // IsCorrect stays null
                }

                if (answer.SelectedChoiceId.HasValue && answer.SelectedChoice is not null)
                {
                    bool isCorrect        = answer.SelectedChoice.IsCorrect;
                    answer.IsCorrect      = isCorrect;
                    answer.PointsAwarded  = isCorrect ? answer.Question.Points : 0;
                    autoGradedScore      += answer.PointsAwarded.Value;
                }
                else
                {
                    answer.IsCorrect     = false;
                    answer.PointsAwarded = 0;
                }
                answer.UpdatedAt = now;
            }

            attempt.AutoGradedScore = autoGradedScore;

            if (hasEssayQuestions)
            {
                attempt.AttemptStatus = AttemptStatus.PendingGrading;
            }
            else
            {
                attempt.EssayScore    = 0;
                attempt.FinalScore    = autoGradedScore;
                attempt.IsPassed      = attempt.MaxScore > 0 &&
                                        ((decimal)autoGradedScore / attempt.MaxScore * 100m)
                                        >= attempt.Exam.PassThresholdPercent;
                attempt.AttemptStatus = AttemptStatus.Graded;
            }

            _logger.LogInformation(
                "Auto-submitted attempt {AttemptId} for student {StudentId} on exam {ExamId}. Status: {Status}.",
                attempt.Id, attempt.StudentId, attempt.ExamId, attempt.AttemptStatus);
        }

        await db.SaveChangesAsync(ct);
    }
}
