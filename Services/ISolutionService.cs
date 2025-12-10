using pyatform.DTOs.Solution;
using pyatform.Models;

namespace pyatform.Services;

public interface ISolutionService
{
    public Task<IEnumerable<SolutionDto>> GetAllSolutionsAsync();
    public Task<SolutionDto?> GetSolutionByIdAsync(int id);
    Task<SolutionDto> AddSolutionAsync(AddSolutionDto addSolutionDto, string userId);
    Task<SolutionDto?> AddTestResultsToSolutionAsync(int id, AddTestResultsToSolutionDto dto);
    Task<bool> DeleteSolutionByIdAsync(int id, string userId);
    Task<IEnumerable<SolutionDto>> GetAllSolutionsForChallengeAsync(int ChallengeId);
    Task<SolutionTestResult> TestSolutionAndSaveAsync(int solutionId);
    Task<SolutionTestResult> TestSolutionAsync(string solutionCode, string testCode);
}
