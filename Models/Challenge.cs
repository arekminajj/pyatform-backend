namespace pyatform.Models;

public class Challenge
{
    public int Id { get; set; }
    public required string Title { get; set; }
    // content stores markdown content
    public string? Content { get; set; }
    public string? TemplateCode { get; set; }
    public string? TestCode { get; set; }
    public int? TimeLimitMs { get; set; }
    public int? MemoryLimitKb { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Solution> Solutions { get; set; } = new List<Solution>();
}
