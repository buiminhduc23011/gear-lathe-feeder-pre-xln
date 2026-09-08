using Desktop.App.Configuration.Plc;
using Desktop.App.Data.Sqlite;
using Microsoft.Data.Sqlite;

namespace Desktop.App.Data.Repositories;

public sealed class PlcParameterSettingsRepository
{
    private readonly string? _databasePath;

    public PlcParameterSettingsRepository(string? databasePath = null)
    {
        _databasePath = databasePath;
    }

    public async Task<IReadOnlyList<PlcParameterSettingRecord>> GetByGroupAsync(string groupName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT tag_name,
                   group_name,
                   value_text,
                   updated_at_utc
            FROM plc_parameter_settings
            WHERE group_name = $groupName
            ORDER BY tag_name;
            """;
        command.Parameters.AddWithValue("$groupName", groupName);

        var records = new List<PlcParameterSettingRecord>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            records.Add(
                new PlcParameterSettingRecord(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    DateTimeOffset.Parse(reader.GetString(3), null, System.Globalization.DateTimeStyles.RoundtripKind)));
        }

        return records;
    }

    public async Task<IReadOnlyList<PlcParameterSettingRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT tag_name,
                   group_name,
                   value_text,
                   updated_at_utc
            FROM plc_parameter_settings
            ORDER BY group_name, tag_name;
            """;

        var records = new List<PlcParameterSettingRecord>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            records.Add(
                new PlcParameterSettingRecord(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    DateTimeOffset.Parse(reader.GetString(3), null, System.Globalization.DateTimeStyles.RoundtripKind)));
        }

        return records;
    }

    public async Task UpsertAsync(string tagName, string groupName, string valueText, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        valueText ??= string.Empty;

        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO plc_parameter_settings
            (
                tag_name,
                group_name,
                value_text,
                updated_at_utc
            )
            VALUES
            (
                $tagName,
                $groupName,
                $valueText,
                $updatedAtUtc
            )
            ON CONFLICT(tag_name) DO UPDATE SET
                group_name = excluded.group_name,
                value_text = excluded.value_text,
                updated_at_utc = excluded.updated_at_utc;
            """;
        command.Parameters.AddWithValue("$tagName", tagName);
        command.Parameters.AddWithValue("$groupName", groupName);
        command.Parameters.AddWithValue("$valueText", valueText);
        command.Parameters.AddWithValue("$updatedAtUtc", DateTimeOffset.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

public sealed record PlcParameterSettingRecord(
    string TagName,
    string GroupName,
    string ValueText,
    DateTimeOffset UpdatedAtUtc);
