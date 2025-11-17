
using pyatform.DTOs.Challenge;
using pyatform.Models;

namespace pyatform.Services;

public interface IChallengeService
{
    public Task<IEnumerable<ChallengeDto>> GetAllChallengesAsync();
    public Task<ChallengeDto?> GetChallengeByIdAsync(int id);
    Task<ChallengeDto> AddChallengeAsync(AddChallengeDto dto, string userId);
    Task<ChallengeDto?> EditChallengeByIdAsync(int id, AddChallengeDto dto, string userId);
    Task<bool> DeleteChallengeByIdAsync(int id, string userId);
}
