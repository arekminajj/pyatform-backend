using pyatform.Data;
using pyatform.DTOs.User;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IEnumerable<TopUser>> GetTopUsersRanking(int ranking_limit)
    {
        // This expects procedure 'top_users' to be added to the database, which is stored inside /misc in top_users.funcion.sql.

        var results = await _ctx.TopUser
        .FromSqlInterpolated($"SELECT * FROM top_users({ranking_limit})")
        .ToListAsync();

        return results;
    }
}
