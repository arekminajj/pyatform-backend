using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using pyatform.Models;
using pyatform.Services;
using pyatform.DTOs.Solution;

namespace pyatform.Controllers;


[ApiController]
[Route("api/solution")]
public class SolutionController : ControllerBase
{
    private readonly ISolutionService _solutionService;
    private readonly UserManager<User> _userManager;
    public SolutionController(ISolutionService solutionService, UserManager<User> userManager)
    {
        _solutionService = solutionService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolutionDto>>> GetSolutions()
    {
        var solutions = await _solutionService.GetAllSolutionsAsync();

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
    public async Task<ActionResult<SolutionTestResult>> TestSolution(int id)
    {
        var result = await _solutionService.TestSolutionAndSaveAsync(id);

        return Ok(result);
    }
}
