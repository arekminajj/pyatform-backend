using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.EntityFrameworkCore;
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

        return new SolutionDto
        {
            Id = solution.Id,
            UserId = solution.UserId,
            ChallengeId = solution.ChallengeId,
            Content = solution.Content,
            ExecutionTime = solution.ExecutionTime,
            MemoryUsed = solution.MemoryUsed,
            HasPassedTests = solution.HasPassedTests,
            SubmissionTime = solution.SubmissionTime
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

    public async Task<IEnumerable<SolutionDto>> GetAllSolutionsAsync()
    {
        var solutions = await _ctx.Solutions.ToListAsync();

        return solutions.Select(s => new SolutionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            ChallengeId = s.ChallengeId,
            Content = s.Content,
            ExecutionTime = s.ExecutionTime,
            MemoryUsed = s.MemoryUsed,
            HasPassedTests = s.HasPassedTests,
            SubmissionTime = s.SubmissionTime
        });
    }

    public async Task<IEnumerable<SolutionDto>> GetAllSolutionsForChallengeAsync(int ChallengeId)
    {
        var solutions = await _ctx.Solutions.Where(s => s.ChallengeId == ChallengeId).ToListAsync();

        return solutions.Select(s => new SolutionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            ChallengeId = s.ChallengeId,
            Content = s.Content,
            ExecutionTime = s.ExecutionTime,
            MemoryUsed = s.MemoryUsed,
            HasPassedTests = s.HasPassedTests,
            SubmissionTime = s.SubmissionTime
        });
    }

    public async Task<SolutionDto?> GetSolutionByIdAsync(int id)
    {
        var solution = await _ctx.Solutions.FirstOrDefaultAsync(s => s.Id == id);

        if (solution == null)
            return null;

        return new SolutionDto
        {
            Id = solution.Id,
            UserId = solution.UserId,
            ChallengeId = solution.ChallengeId,
            Content = solution.Content,
            ExecutionTime = solution.ExecutionTime,
            MemoryUsed = solution.MemoryUsed,
            HasPassedTests = solution.HasPassedTests,
            SubmissionTime = solution.SubmissionTime
        };
    }

    public async Task<SolutionTestResult> TestSolutionAsync(Solution solution)
    {
        var solutionCode = solution.Content;
        var testCode = solution.Challenge?.TestCode;

        if (testCode == null)
        {
            throw new NullReferenceException(
                "The solution is not associated with a challenge or the challenge has no test code."
            );
        }

        //todo: check new TempDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var solutionfilePath = Path.Combine(tempDir, "solution_script.py");
        await File.WriteAllTextAsync(solutionfilePath, solutionCode);
        var testFilePath = Path.Combine(tempDir, "test.py");
        await File.WriteAllTextAsync(testFilePath, testCode);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                ArgumentList = {
                    "run", "--rm",
                    "-v", $"{tempDir}:/app",
                    "-w", "/app",
                    "python:3.11",
                    "python", "test.py"
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = Process.Start(psi);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start the docker process.");
            }
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

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
