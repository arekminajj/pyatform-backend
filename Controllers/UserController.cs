using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using pyatform.Data;
using pyatform.DTOs.User;
using pyatform.Models;
using pyatform.Services;

namespace pyatform.Controllers;


[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    public UserController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        return Ok(new UserDto
        {
            Id = user.Id,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Bio = user.Bio,
            Email = user.Email
        });
    }

    [HttpPost("upload-pfp")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UploadProfilePicture(IFormFile file,
        [FromServices] IBlobService blobService)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        const long MaxFileSize = 5 * 1024 * 1024;
        if (file.Length > MaxFileSize)
            return BadRequest($"File size cannot exceed 5 MB.");

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var url = await blobService.UploadFileAsync(file);
        user.ProfilePictureUrl = url;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return StatusCode(500, "Failed to update user profile picture.");

        return Ok(new UserDto
        {
            Id = user.Id,
            ProfilePictureUrl = user.ProfilePictureUrl
        });
    }

    [HttpPut("update-bio")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateUserProfileDio dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        user.Bio = dto.Bio;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return StatusCode(500, "Failed to update user bio");

        return Ok(new UserDto
        {
            Id = user.Id,
            Bio = user.Bio,
            Email = user.Email,
            ProfilePictureUrl = user.ProfilePictureUrl
        });
    }
}