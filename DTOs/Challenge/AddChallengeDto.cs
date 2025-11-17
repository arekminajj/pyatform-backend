namespace pyatform.DTOs.Challenge;

public class AddChallengeDto
{
    public required string Title { get; set; }
    public string? Content { get; set; }
    public string? TemplateCode { get; set; }
    public string? TestCode { get; set; }
    public int? TimeLimitMs { get; set; }
    public int? MemoryLimitKb { get; set; }
}