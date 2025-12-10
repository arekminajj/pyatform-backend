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

        SolutionTestResult testResult = await TestSolutionAsync(solution.Content, challange.TestCode!);
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
            SubmissionTime = solution.SubmissionTime
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
            ExecutionTimeMs = s.ExecutionTimeMs,
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
            ExecutionTimeMs = s.ExecutionTimeMs,
            HasPassedTests = s.HasPassedTests,
            SubmissionTime = s.SubmissionTime
        });
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

    public async Task<SolutionTestResult> TestSolutionAndSaveAsync(int solutionId)
    {
        var solution = await _ctx.Solutions
        .Include(s => s.Challenge)
        .FirstOrDefaultAsync(s => s.Id == solutionId);

        if (solution == null)
            throw new KeyNotFoundException($"Solution of id: {solutionId} was not found");

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
                    "-v", $"{tempDir}:/app",
                    "-w", "/app",
                    "pyatform-python-pytest",
                    "pytest", "-q", "--disable-warnings", "--maxfail=1", "--timeout=2"
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true
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

            await AddTestResultsToSolutionAsync(solutionId, addResultsDto);
            
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
    
    public async Task<SolutionTestResult> TestSolutionAsync(string solutionCode, string testCode)
    {
        if (string.IsNullOrEmpty(solutionCode))
            throw new Exception("Solution code cannot be null!");
        if (string.IsNullOrEmpty(testCode))
            throw new Exception("Test code cannot be null!");
        
        //todo: check new TempDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var solutionfilePath = Path.Combine(tempDir, "solution_script.py");
        await File.WriteAllTextAsync(solutionfilePath, solutionCode);
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
                    "-v", $"{tempDir}:/app",
                    "-w", "/app",
                    "pyatform-python-pytest",
                    "pytest", "-q", "--disable-warnings", "--maxfail=1", "--timeout=2"
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true
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
