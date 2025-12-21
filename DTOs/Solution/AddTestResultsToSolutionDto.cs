
using pyatform.Models;

namespace pyatform.DTOs.Solution;

public class AddTestResultsToSolutionDto
{
    public int? ExecutionTimeMs { get; set; }
    public bool HasPassedTests { get; set; }
    public pyatform.Models.TestResult? TestResult { get; set; }
}
