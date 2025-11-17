namespace pyatform.Models;

public class Solution
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }
    public int ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }
    // content stores code
    public required string Content { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public bool HasPassedTests { get; set; }
    public DateTime SubmissionTime { get; set; } = DateTime.UtcNow;
}
