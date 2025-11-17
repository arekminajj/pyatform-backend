
namespace pyatform.DTOs.Solution;

public class SolutionDto
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int ChallengeId { get; set; }
    // content stores code
    public required string Content { get; set; }
    public int? ExecutionTime { get; set; }
    public int? MemoryUsed { get; set; }
    public bool HasPassedTests { get; set; }
    public DateTime SubmissionTime { get; set; } = DateTime.UtcNow;
}
