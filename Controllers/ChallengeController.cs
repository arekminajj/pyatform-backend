using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using pyatform.Models;
using pyatform.Services;
using pyatform.DTOs.Challenge;

namespace pyatform.Controllers;


[ApiController]
[Route("api/challenge")]
public class ChallengeController : ControllerBase
{
    private readonly IChallengeService _challengeService;
    private readonly UserManager<User> _userManager;
    public ChallengeController(IChallengeService challengeService, UserManager<User> userManager)
    {
        _challengeService = challengeService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChallengeDto>>> GetChalanges()
    {
        var challenges = await _challengeService.GetAllChallengesAsync();

        return Ok(challenges);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChallengeDto>> GetChallengeById(int id)
    {
        var challenge = await _challengeService.GetChallengeByIdAsync(id);

        if (challenge == null)
            return NotFound();

        return Ok(challenge);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ChallengeDto>> AddChallenge([FromBody] AddChallengeDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;

        if (userId == null)
            return Unauthorized();

        var newChallenge = await _challengeService.AddChallengeAsync(dto, userId);

        return CreatedAtAction(nameof(GetChallengeById),
            new { id = newChallenge.Id }, newChallenge);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<Challenge>> EditChallenge(int id, [FromBody] AddChallengeDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;
        if (userId == null) return Unauthorized();

        var updated = await _challengeService.EditChallengeByIdAsync(id, dto, userId);
        if (updated == null) return NotFound();

        return Ok(updated);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteChallenge(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;
        if (userId == null) return Unauthorized();

        var result = await _challengeService.DeleteChallengeByIdAsync(id, userId);
        if (!result) return NotFound();
        return NoContent();
    } 
}
