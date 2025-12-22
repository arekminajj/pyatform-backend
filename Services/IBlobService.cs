using pyatform.DTOs.Solution;
using pyatform.DTOs.TestResult;
using pyatform.Models;

namespace pyatform.Services;

public interface IBlobService
{
    Task<string> UploadFileAsync(IFormFile file);
}
