using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using pyatform.Models;
using pyatform.Services;
using pyatform.DTOs.Solution;
using Npgsql.Internal;

namespace pyatform.Controllers;


[ApiController]
[Route("api/solution")]
public class SolutionController : ControllerBase
{
    private readonly ISolutionService _solutionService;
    private readonly IChallengeService _challangeService;
    private readonly UserManager<User> _userManager;
    public SolutionController(ISolutionService solutionService, IChallengeService challengeService, UserManager<User> userManager)
    {
        _solutionService = solutionService;
        _userManager = userManager;
        _challangeService = challengeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolutionDto>>> GetSolutions(
        [FromQuery] int? challengeId,
        [FromQuery] string? userId)
    {
        var solutions = await _solutionService.GetSolutionsAsync(challengeId, userId);

        return Ok(solutions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SolutionDto>> GetSolutionById(int id)
    {
        var solution = await _solutionService.GetSolutionByIdAsync(id);

        if (solution == null)
            return NotFound();

        return Ok(solution);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SolutionDto>> AddSolution([FromBody] AddSolutionDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;

        if (userId == null)
            return Unauthorized();

        var solution = await _solutionService.AddSolutionAsync(dto, userId);

        return CreatedAtAction(nameof(GetSolutionById),
            new { id = solution.Id }, solution);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSolution(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;
        if (userId == null) return Unauthorized();

        var result = await _solutionService.DeleteSolutionByIdAsync(id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("test/{id}")]
    [Authorize]
    public async Task<ActionResult<TestResult>> TestSolution(int id)
    {
        var solution = await _solutionService.GetSolutionByIdAsync(id);
        if (solution == null) return NotFound();
        var challange = await _challangeService.GetChallengeByIdAsync(solution.ChallengeId);
        if (challange == null) throw new InvalidOperationException("Challenge missing");

        var testResult = await _solutionService.TestSolutionAsync(solution.Id, challange.TestCode!);

        return Ok(testResult);
    }
}
