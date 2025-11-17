using Microsoft.AspNetCore.Identity;

namespace pyatform.Models;

public class User : IdentityUser
{
    public string? ProfilePictureUrl { get; set; }

    public ICollection<Solution> Solutions { get; set; } = new List<Solution>();
    public ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();
}
