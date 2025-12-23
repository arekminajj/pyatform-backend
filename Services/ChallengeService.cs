

using pyatform.Data;
using pyatform.Models;
using pyatform.DTOs;
using pyatform.DTOs.Challenge;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace pyatform.Services;

public class ChallengeService : IChallengeService
{
    private readonly ApplicationDbContext _ctx;
    public ChallengeService(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<ChallengeDto> AddChallengeAsync(AddChallengeDto dto, string userId)
    {
        var challenge = new Challenge
        {
            Title = dto.Title,
            Content = dto.Content,
            TemplateCode = dto.TemplateCode,
            TestCode = dto.TestCode,
            TimeLimitMs = dto.TimeLimitMs,
            MemoryLimitKb = dto.MemoryLimitKb,
            UserId = userId
        };

        await _ctx.Challenges.AddAsync(challenge);
        await _ctx.SaveChangesAsync();

        return new ChallengeDto
        {
            Id = challenge.Id,
            Title = challenge.Title,
            Content = challenge.Content,
            TemplateCode = challenge.TemplateCode,
            TestCode = challenge.TestCode,
            TimeLimitMs = challenge.TimeLimitMs,
            MemoryLimitKb = challenge.MemoryLimitKb,
            UserId = challenge.UserId
        };
    }

    public async Task<bool> DeleteChallengeByIdAsync(int id, string userId)
    {
        var challenge = await _ctx.Challenges
            .FirstOrDefaultAsync(c => c.Id == id);

        if (challenge == null)
            return false;

        if (challenge.UserId != userId)
            return false;

        _ctx.Challenges.Remove(challenge);
        await _ctx.SaveChangesAsync();

        return true;
    }

    public async Task<ChallengeDto?> EditChallengeByIdAsync(int id, AddChallengeDto dto, string userId)
    {
        var challenge = await _ctx.Challenges
            .FirstOrDefaultAsync(c => c.Id == id);

        if (challenge == null)
            return null;

        if (challenge.UserId != userId)
            return null;

        challenge.Title = dto.Title;
        challenge.Content = dto.Content;
        challenge.TemplateCode = dto.TemplateCode;
        challenge.TestCode = dto.TestCode;
        challenge.TimeLimitMs = dto.TimeLimitMs;
        challenge.MemoryLimitKb = dto.MemoryLimitKb;

        await _ctx.SaveChangesAsync();

        return new ChallengeDto
        {
            Id = challenge.Id,
            Title = challenge.Title,
            Content = challenge.Content,
            TemplateCode = challenge.TemplateCode,
            TestCode = challenge.TestCode,
            TimeLimitMs = challenge.TimeLimitMs,
            MemoryLimitKb = challenge.MemoryLimitKb,
            UserId = challenge.UserId
        };
    }

    public async Task<IEnumerable<ChallengeDto>> GetAllChallengesAsync(string? userId)
    {
        return await _ctx.Challenges
        .Select(c => new ChallengeDto
        {
            Id = c.Id,
            Title = c.Title,
            Content = c.Content,
            TemplateCode = c.TemplateCode,
            TestCode = c.TestCode,
            TimeLimitMs = c.TimeLimitMs,
            MemoryLimitKb = c.MemoryLimitKb,
            UserId = c.UserId,
            isCompletedByUser = userId != null 
                ? c.Solutions.Any(s => s.UserId == userId && s.HasPassedTests) 
                : null
        })
        .ToListAsync();
    }

    public async Task<ChallengeDto?> GetChallengeByIdAsync(int id, string? userId)
    {
        var challenge = await _ctx.Challenges.FirstOrDefaultAsync(c => c.Id == id);

        if (challenge == null)
            return null;

        return new ChallengeDto
        {
            Id = challenge.Id,
            Title = challenge.Title,
            Content = challenge.Content,
            TemplateCode = challenge.TemplateCode,
            TestCode = challenge.TestCode,
            TimeLimitMs = challenge.TimeLimitMs,
            MemoryLimitKb = challenge.MemoryLimitKb,
            UserId = challenge.UserId,
            isCompletedByUser = userId != null 
                ? challenge.Solutions.Any(s => s.UserId == userId && s.HasPassedTests) 
                : null
        };
    }
}
