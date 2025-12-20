using pyatform.DTOs.Solution;
using pyatform.Models;

namespace pyatform.Services;

public interface ISolutionService
{
    public Task<IEnumerable<SolutionDto>> GetSolutionsAsync(int? challengeId = null, string? userId = null);
    public Task<SolutionDto?> GetSolutionByIdAsync(int id);
    Task<SolutionDto> AddSolutionAsync(AddSolutionDto addSolutionDto, string userId);
    Task<SolutionDto?> AddTestResultsToSolutionAsync(int id, AddTestResultsToSolutionDto dto);
    Task<bool> DeleteSolutionByIdAsync(int id, string userId);
    Task<SolutionTestResult> TestSolutionAsync(int solutionId, string testCode);
}
