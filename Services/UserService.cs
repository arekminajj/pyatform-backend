using pyatform.Data;
using pyatform.DTOs.User;

namespace pyatform.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _ctx;

    public UserService(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<UserDto> GetUserByIdAsync(string id)
    {
        var user = _ctx.Users.FirstOrDefault(u => u.Id == id);
        if (user == null) throw new Exception($"User with id {id} does not exist");

        return new UserDto
        {
            Id = user.Id,
            Bio = user.Bio,
            Email = user.Email,
            ProfilePictureUrl = user.ProfilePictureUrl  
        };
    }
}
