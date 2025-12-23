namespace pyatform.DTOs.User;

public class TopUser
{
    public required string UserId { get; set; }
    public required string UserName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public long ChallengesCount { get; set; }
}
