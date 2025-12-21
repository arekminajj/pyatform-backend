namespace pyatform.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public int ReturnCode { get; set; }
        public string? Error { get; set; }
        public string? Output { get; set; }
        public int? ExecutionTimeMs { get; set; }
        public int SolutionId { get; set; }
        public Solution? Solution { get; set; }
    }
}
