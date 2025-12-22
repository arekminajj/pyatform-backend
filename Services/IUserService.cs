using pyatform.DTOs.Solution;
using pyatform.DTOs.TestResult;
using pyatform.DTOs.User;
using pyatform.Models;

namespace pyatform.Services;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(string id);
}
