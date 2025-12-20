using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pyatform.Data;
using pyatform.DTOs.Solution;
using pyatform.Models;

namespace pyatform.Services;

public class SolutionService : ISolutionService
{
    private readonly ApplicationDbContext _ctx;
    public SolutionService(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }
    public async Task<SolutionDto> AddSolutionAsync(AddSolutionDto addSolutionDto, string userId)
    {
        var challange = await _ctx.Challenges.FirstOrDefaultAsync(c => c.Id == addSolutionDto.ChallengeId);

        if (challange == null)
             throw new KeyNotFoundException($"Challenge {addSolutionDto.ChallengeId} not found.");

        var solution = new Solution
        {
            UserId = userId,
            ChallengeId = addSolutionDto.ChallengeId,
            Content = addSolutionDto.Content,
            SubmissionTime = DateTime.UtcNow
        };

        await _ctx.Solutions.AddAsync(solution);
        await _ctx.SaveChangesAsync();

        SolutionTestResult testResult = await TestSolutionAsync(solution.Id, challange.TestCode!);
        await AddTestResultsToSolutionAsync(solution.Id, new AddTestResultsToSolutionDto
        {
            ExecutionTimeMs = 0,
            HasPassedTests = testResult.ReturnCode == 0
        });

        return new SolutionDto
        {
            Id = solution.Id,
            UserId = solution.UserId,
            ChallengeId = solution.ChallengeId,
            Content = solution.Content,
            ExecutionTimeMs = solution.ExecutionTimeMs,
            HasPassedTests = solution.HasPassedTests,
            SubmissionTime = solution.SubmissionTime,
            Output = testResult.Output
        };
    }

    public async Task<SolutionDto?> AddTestResultsToSolutionAsync(int solutionId, AddTestResultsToSolutionDto dto)
    {
        var solution = await _ctx.Solutions
            .FirstOrDefaultAsync(s => s.Id == solutionId);

        if (solution == null)
            return null;

        solution.ExecutionTimeMs = dto.ExecutionTimeMs;
        solution.HasPassedTests = dto.HasPassedTests;

        await _ctx.SaveChangesAsync();

        return new SolutionDto
        {
            Id = solution.Id,
            UserId = solution.UserId,
            ChallengeId = solution.ChallengeId,
            Content = solution.Content,
            ExecutionTimeMs = solution.ExecutionTimeMs,
            HasPassedTests = solution.HasPassedTests,
            SubmissionTime = solution.SubmissionTime,
        };
    }

    public async Task<bool> DeleteSolutionByIdAsync(int id, string userId)
    {
        var solution = await _ctx.Solutions
            .FirstOrDefaultAsync(c => c.Id == id);

        if (solution == null)
            return false;

        if (solution.UserId != userId)
            return false;

        _ctx.Solutions.Remove(solution);
        await _ctx.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<SolutionDto>> GetSolutionsAsync(int? challengeId, string? userId)
    {
        IQueryable<Solution> query = _ctx.Solutions;

        if (challengeId.HasValue)
            query = query.Where(s => s.ChallengeId == challengeId);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(s => s.UserId == userId);

        return await query
            .Select(s => new SolutionDto
            {
                Id = s.Id,
                UserId = s.UserId,
                ChallengeId = s.ChallengeId,
                Content = s.Content,
                ExecutionTimeMs = s.ExecutionTimeMs,
                HasPassedTests = s.HasPassedTests,
                SubmissionTime = s.SubmissionTime
            })
            .ToListAsync();
    }

    public async Task<SolutionDto?> GetSolutionByIdAsync(int id)
    {
        var solution = await _ctx.Solutions
        .FirstOrDefaultAsync(s => s.Id == id);

        if (solution == null)
            return null;

        return new SolutionDto
        {
            Id = solution.Id,
            UserId = solution.UserId,
            ChallengeId = solution.ChallengeId,
            Content = solution.Content,
            ExecutionTimeMs = solution.ExecutionTimeMs,
            HasPassedTests = solution.HasPassedTests,
            SubmissionTime = solution.SubmissionTime
        };
    }
   
    public async Task<SolutionTestResult> TestSolutionAsync(int solutionId, string testCode)
    {
        var solution = _ctx.Solutions.FirstOrDefault(s => s.Id == solutionId);
        if (solution == null)
            throw new Exception($"Does not exist a solution with id:{solutionId}");

        if (string.IsNullOrEmpty(testCode))
            throw new Exception("Test code cannot be null");
        
        //todo: check new TempDirectory();
        var tempDir = Path.Combine("/tmp", Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var solutionfilePath = Path.Combine(tempDir, "solution_script.py");
        await File.WriteAllTextAsync(solutionfilePath, solution.Content);
        var testFilePath = Path.Combine(tempDir, "test_script.py");
        await File.WriteAllTextAsync(testFilePath, testCode);
        string containerName = $"pyatform-test-{Guid.NewGuid()}";

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                ArgumentList = {
                    "run", "--rm", "--name", containerName,
                    "-v", $"{tempDir}:/tmp",
                    "-w", "/tmp",
                    "pyatform-python-pytest",
                    "pytest", "-q", "--disable-warnings", "--maxfail=1", "--timeout=2"
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = tempDir
            };

            var sw = Stopwatch.StartNew();

            var process = Process.Start(psi);
            if (process == null)
                throw new InvalidOperationException("Failed to start the docker process.");

            await process.WaitForExitAsync();
            sw.Stop();
            var ExecutionTimeMs = sw.ElapsedMilliseconds;

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var addResultsDto = new AddTestResultsToSolutionDto
            {
                ExecutionTimeMs = (int)ExecutionTimeMs,
                HasPassedTests = process.ExitCode == 0
            };

            await AddTestResultsToSolutionAsync(solution.Id, addResultsDto);
            
            return new SolutionTestResult
            {
                ReturnCode = process.ExitCode,
                Output = output,
                Error = error
            };
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
