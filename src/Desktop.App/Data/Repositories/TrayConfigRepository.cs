using Desktop.App.Data.Sqlite;
using Desktop.App.Models.Tray;
using Microsoft.Data.Sqlite;

namespace Desktop.App.Data.Repositories;

public interface ITrayConfigRepository
{
    Task<List<TrayConfig>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TrayConfig?> GetAsync(TraySize size, CancellationToken cancellationToken = default);
    Task SaveAsync(TrayConfig config, CancellationToken cancellationToken = default);
}

public sealed class TrayConfigRepository : ITrayConfigRepository
{
    private readonly string? _databasePath;

    public TrayConfigRepository(string? databasePath = null)
    {
        _databasePath = databasePath;
    }

    public async Task<List<TrayConfig>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, tray_size, rows, columns, row_offset, col_offset, updated_at_utc
            FROM tray_configs
            ORDER BY tray_size;
            """;

        var results = new List<TrayConfig>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(ReadTrayConfig(reader));
        }

        return results;
    }

    public async Task<TrayConfig?> GetAsync(TraySize size, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, tray_size, rows, columns, row_offset, col_offset, updated_at_utc
            FROM tray_configs
            WHERE tray_size = $traySize
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$traySize", (int)size);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return ReadTrayConfig(reader);
        }

        return null;
    }

    public async Task SaveAsync(TrayConfig config, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO tray_configs (tray_size, rows, columns, row_offset, col_offset, updated_at_utc)
            VALUES ($traySize, $rows, $columns, $rowOffset, $colOffset, $updatedAtUtc)
            ON CONFLICT(tray_size) DO UPDATE SET
                rows = excluded.rows,
                columns = excluded.columns,
                row_offset = excluded.row_offset,
                col_offset = excluded.col_offset,
                updated_at_utc = excluded.updated_at_utc;
            """;

        command.Parameters.AddWithValue("$traySize", (int)config.Size);
        command.Parameters.AddWithValue("$rows", config.Rows);
        command.Parameters.AddWithValue("$columns", config.Columns);
        command.Parameters.AddWithValue("$rowOffset", config.RowOffset);
        command.Parameters.AddWithValue("$colOffset", config.ColOffset);
        command.Parameters.AddWithValue("$updatedAtUtc", DateTimeOffset.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static TrayConfig ReadTrayConfig(SqliteDataReader reader)
    {
        return new TrayConfig
        {
            Id = reader.GetInt32(0),
            Size = (TraySize)reader.GetInt32(1),
            Rows = reader.GetInt32(2),
            Columns = reader.GetInt32(3),
            RowOffset = reader.GetFloat(4),
            ColOffset = reader.GetFloat(5),
            UpdatedAtUtc = DateTime.Parse(reader.GetString(6)).ToUniversalTime()
        };
    }
}
