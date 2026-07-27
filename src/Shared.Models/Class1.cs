namespace Shared.Models;

public sealed class RobotJobInfo
{
	public string MachineName { get; set; } = string.Empty;

	public string CurrentOperation { get; set; } = string.Empty;

	public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}
