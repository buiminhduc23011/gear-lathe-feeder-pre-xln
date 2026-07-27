using Desktop.App.Data.Sqlite;
using Microsoft.Data.Sqlite;

namespace Desktop.App.Data.Repositories;

public interface IAuditLogRepository
{
    Task LogAsync(string action, string username, string? detail = null, CancellationToken cancellationToken = default);
}

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly string? _databasePath;

    public AuditLogRepository(string? databasePath = null)
    {
        _databasePath = databasePath;
    }

    public async Task LogAsync(string action, string username, string? detail = null, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO audit_log (action, username, detail, created_at_utc)
            VALUES ($action, $username, $detail, $createdAtUtc);
            """;

        command.Parameters.AddWithValue("$action", action);
        command.Parameters.AddWithValue("$username", username);
        command.Parameters.AddWithValue("$detail", string.IsNullOrWhiteSpace(detail) ? (object)DBNull.Value : detail);
        command.Parameters.AddWithValue("$createdAtUtc", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
