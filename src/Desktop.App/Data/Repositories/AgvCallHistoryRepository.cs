using Desktop.App.Data.Sqlite;
using Desktop.App.Models.Agv;
using Microsoft.Data.Sqlite;

namespace Desktop.App.Data.Repositories;

public interface IAgvCallHistoryRepository
{
    Task<int> SaveAsync(AgvCallRecord record, CancellationToken cancellationToken = default);
    Task UpdateAsync(AgvCallRecord record, CancellationToken cancellationToken = default);
    Task<List<AgvCallRecord>> GetRecentAsync(AgvPosition? filterPosition = null, DateTime? date = null, int limit = 100, CancellationToken cancellationToken = default);
    Task<AgvCallHistoryPageResult> GetPagedAsync(AgvPosition? filterPosition, DateTime? date, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> DeleteByFilterAsync(AgvPosition? filterPosition, DateTime? date, CancellationToken cancellationToken = default);
}

public sealed class AgvCallHistoryRepository : IAgvCallHistoryRepository
{
    private readonly string? _databasePath;

    public AgvCallHistoryRepository(string? databasePath = null)
    {
        _databasePath = databasePath;
    }

    public async Task<int> SaveAsync(AgvCallRecord record, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO agv_call_history
            (
                ke_type,
                status,
                is_auto_call,
                created_at_utc,
                started_at_utc,
                completed_at_utc,
                note,
                remaining_qty
            )
            VALUES
            (
                $position,
                $status,
                $isAutoCall,
                $createdAtUtc,
                $startedAtUtc,
                $completedAtUtc,
                $note,
                $remainingQty
            );
            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue("$position", (int)record.Position);
        command.Parameters.AddWithValue("$status", (int)record.Status);
        command.Parameters.AddWithValue("$isAutoCall", record.IsAutoCall ? 1 : 0);
        command.Parameters.AddWithValue("$createdAtUtc", record.CreatedAtUtc.ToString("O"));
        command.Parameters.AddWithValue("$startedAtUtc", record.StartedAtUtc?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$completedAtUtc", record.CompletedAtUtc?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$note", string.IsNullOrWhiteSpace(record.Note) ? DBNull.Value : record.Note.Trim());
        command.Parameters.AddWithValue("$remainingQty", record.RemainingQty);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        int id = Convert.ToInt32(result);
        record.Id = id;
        return id;
    }

    public async Task UpdateAsync(AgvCallRecord record, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE agv_call_history SET
                status = $status,
                started_at_utc = $startedAtUtc,
                completed_at_utc = $completedAtUtc,
                note = $note,
                remaining_qty = $remainingQty
            WHERE id = $id;
            """;

        command.Parameters.AddWithValue("$id", record.Id);
        command.Parameters.AddWithValue("$status", (int)record.Status);
        command.Parameters.AddWithValue("$startedAtUtc", record.StartedAtUtc?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$completedAtUtc", record.CompletedAtUtc?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$note", string.IsNullOrWhiteSpace(record.Note) ? DBNull.Value : record.Note.Trim());
        command.Parameters.AddWithValue("$remainingQty", record.RemainingQty);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<AgvCallRecord>> GetRecentAsync(AgvPosition? filterPosition = null, DateTime? date = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        var sql =
            """
            SELECT id,
                   ke_type,
                   status,
                   is_auto_call,
                   created_at_utc,
                   started_at_utc,
                   completed_at_utc,
                   note,
                   remaining_qty
            FROM agv_call_history
            WHERE 1=1
            """;

        if (filterPosition.HasValue)
        {
            sql += " AND ke_type = $position";
            command.Parameters.AddWithValue("$position", (int)filterPosition.Value);
        }

        if (date.HasValue)
        {
            var dateText = date.Value.ToString("yyyy-MM-dd");
            sql += " AND created_at_utc LIKE $dateText || '%'";
            command.Parameters.AddWithValue("$dateText", dateText);
        }

        sql += " ORDER BY created_at_utc DESC LIMIT $limit;";
        command.CommandText = sql;
        command.Parameters.AddWithValue("$limit", limit);

        return await ReadRecordsAsync(command, cancellationToken);
    }

    public async Task<AgvCallHistoryPageResult> GetPagedAsync(AgvPosition? filterPosition, DateTime? date, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 500);
        var offset = (pageNumber - 1) * pageSize;

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        // Count
        var countCommand = connection.CreateCommand();
        var whereClause = BuildWhereClause(filterPosition, date, countCommand);
        countCommand.CommandText = $"SELECT COUNT(*) FROM agv_call_history {whereClause};";

        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));
        if (totalCount == 0)
        {
            return new AgvCallHistoryPageResult();
        }

        // Data
        var command = connection.CreateCommand();
        whereClause = BuildWhereClause(filterPosition, date, command);

        command.CommandText =
            $"""
            SELECT id,
                   ke_type,
                   status,
                   is_auto_call,
                   created_at_utc,
                   started_at_utc,
                   completed_at_utc,
                   note,
                   remaining_qty
            FROM agv_call_history
            {whereClause}
            ORDER BY created_at_utc DESC
            LIMIT $limit OFFSET $offset;
            """;

        command.Parameters.AddWithValue("$limit", pageSize);
        command.Parameters.AddWithValue("$offset", offset);

        var items = await ReadRecordsAsync(command, cancellationToken);
        return new AgvCallHistoryPageResult
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    public async Task<int> DeleteByFilterAsync(AgvPosition? filterPosition, DateTime? date, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        var whereClause = BuildWhereClause(filterPosition, date, command);
        command.CommandText = $"DELETE FROM agv_call_history {whereClause};";

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string BuildWhereClause(AgvPosition? filterPosition, DateTime? date, SqliteCommand command)
    {
        var filters = new List<string>();

        if (filterPosition.HasValue)
        {
            filters.Add("ke_type = $position");
            command.Parameters.AddWithValue("$position", (int)filterPosition.Value);
        }

        if (date.HasValue)
        {
            var dateText = date.Value.ToString("yyyy-MM-dd");
            filters.Add("created_at_utc LIKE $dateText || '%'");
            command.Parameters.AddWithValue("$dateText", dateText);
        }

        return filters.Count > 0 ? $"WHERE {string.Join(" AND ", filters)}" : string.Empty;
    }

    private static async Task<List<AgvCallRecord>> ReadRecordsAsync(SqliteCommand command, CancellationToken cancellationToken)
    {
        var results = new List<AgvCallRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var createdAtText = reader.GetString(4);
            var startedAtText = reader.IsDBNull(5) ? null : reader.GetString(5);
            var completedAtText = reader.IsDBNull(6) ? null : reader.GetString(6);

            results.Add(new AgvCallRecord
            {
                Id = reader.GetInt32(0),
                Position = (AgvPosition)reader.GetInt32(1),
                Status = (AgvCallStatus)reader.GetInt32(2),
                IsAutoCall = !reader.IsDBNull(3) && reader.GetInt32(3) != 0,
                CreatedAtUtc = DateTime.Parse(createdAtText).ToUniversalTime(),
                StartedAtUtc = startedAtText != null ? DateTime.Parse(startedAtText).ToUniversalTime() : null,
                CompletedAtUtc = completedAtText != null ? DateTime.Parse(completedAtText).ToUniversalTime() : null,
                Note = reader.IsDBNull(7) ? null : reader.GetString(7),
                RemainingQty = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
            });
        }

        return results;
    }
}

public sealed class AgvCallHistoryPageResult
{
    public List<AgvCallRecord> Items { get; init; } = [];
    public int TotalCount { get; init; }
}
