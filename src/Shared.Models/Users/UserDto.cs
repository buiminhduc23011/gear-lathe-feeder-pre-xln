namespace Shared.Models.Users;

public sealed class UserDto
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTimeOffset LastSeenUtc { get; set; } = DateTimeOffset.UtcNow;
}
