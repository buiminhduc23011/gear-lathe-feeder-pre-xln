using System.Globalization;
using Desktop.App.Configuration.Alarms;
using Desktop.App.Data.Sqlite;
using Desktop.App.Models.Alarms;
using Desktop.App.Services.Abstractions;
using Microsoft.Data.Sqlite;

namespace Desktop.App.Data.Repositories;

public sealed class AlarmHistoryRepository : IAlarmHistoryRepository
{
    private readonly string? _databasePath;

    public AlarmHistoryRepository(string? databasePath = null)
    {
        _databasePath = databasePath;
    }

    public async Task<long> InsertActiveAsync(AlarmHistoryItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO alarm_history
            (
                alarm_key,
                tag_name,
                address,
                code_value,
                title,
                description,
                remedy,
                alarm_type,
                severity,
                status,
                stop_machine,
                started_at_utc,
                ended_at_utc,
                duration_seconds,
                machine_code,
                raw_value,
                created_at_utc,
                updated_at_utc
            )
            VALUES
            (
                $alarmKey,
                $tagName,
                $address,
                $codeValue,
                $title,
                $description,
                $remedy,
                $alarmType,
                $severity,
                $status,
                $stopMachine,
                $startedAtUtc,
                NULL,
                NULL,
                $machineCode,
                $rawValue,
                $createdAtUtc,
                $updatedAtUtc
            );

            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue("$alarmKey", item.AlarmKey);
        command.Parameters.AddWithValue("$tagName", item.TagName);
        command.Parameters.AddWithValue("$address", item.Address);
        command.Parameters.AddWithValue("$codeValue", item.CodeValue is null ? DBNull.Value : item.CodeValue.Value);
        command.Parameters.AddWithValue("$title", item.Title ?? string.Empty);
        command.Parameters.AddWithValue("$description", AlarmDefinitionCatalog.NormalizeDescription(item.Description));
        command.Parameters.AddWithValue("$remedy", AlarmDefinitionCatalog.NormalizeRemedy(item.Remedy));
        command.Parameters.AddWithValue("$alarmType", item.AlarmType.ToString());
        command.Parameters.AddWithValue("$severity", item.Severity.ToString());
        command.Parameters.AddWithValue("$status", item.Status.ToString());
        command.Parameters.AddWithValue("$stopMachine", item.StopMachine ? 1 : 0);
        command.Parameters.AddWithValue("$startedAtUtc", ToUtcText(item.StartedAtUtc));
        command.Parameters.AddWithValue("$machineCode", item.MachineCode);
        command.Parameters.AddWithValue("$rawValue", item.RawValue);
        command.Parameters.AddWithValue("$createdAtUtc", ToUtcText(item.CreatedAtUtc));
        command.Parameters.AddWithValue("$updatedAtUtc", ToUtcText(item.UpdatedAtUtc));

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    public async Task ResolveAsync(string alarmKey, DateTime endedAtUtc, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(alarmKey))
        {
            return;
        }

        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE alarm_history
            SET status = $resolvedStatus,
                ended_at_utc = $endedAtUtc,
                duration_seconds = CAST(MAX((julianday($endedAtUtc) - julianday(started_at_utc)) * 86400, 0) AS INTEGER),
                updated_at_utc = $updatedAtUtc
            WHERE id = (
                SELECT id
                FROM alarm_history
                WHERE alarm_key = $alarmKey
                  AND status = $activeStatus
                ORDER BY started_at_utc DESC
                LIMIT 1
            );
            """;

        command.Parameters.AddWithValue("$alarmKey", alarmKey);
        command.Parameters.AddWithValue("$activeStatus", AlarmRecordStatus.Active.ToString());
        command.Parameters.AddWithValue("$resolvedStatus", AlarmRecordStatus.Resolved.ToString());
        command.Parameters.AddWithValue("$endedAtUtc", ToUtcText(endedAtUtc));
        command.Parameters.AddWithValue("$updatedAtUtc", ToUtcText(endedAtUtc));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ResolveAllActiveAsync(DateTime endedAtUtc, CancellationToken cancellationToken = default)
    {
        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE alarm_history
            SET status = $resolvedStatus,
                ended_at_utc = $endedAtUtc,
                duration_seconds = CAST(MAX((julianday($endedAtUtc) - julianday(started_at_utc)) * 86400, 0) AS INTEGER),
                updated_at_utc = $updatedAtUtc
            WHERE status = $activeStatus;
            """;

        command.Parameters.AddWithValue("$activeStatus", AlarmRecordStatus.Active.ToString());
        command.Parameters.AddWithValue("$resolvedStatus", AlarmRecordStatus.Resolved.ToString());
        command.Parameters.AddWithValue("$endedAtUtc", ToUtcText(endedAtUtc));
        command.Parameters.AddWithValue("$updatedAtUtc", ToUtcText(endedAtUtc));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<AlarmHistoryPageResult> QueryAsync(AlarmHistoryQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);
        var offset = (pageNumber - 1) * pageSize;
        var countCommand = connection.CreateCommand();
        var whereClause = BuildWhereClause(query, countCommand);
        countCommand.CommandText =
            $"""
            SELECT COUNT(*)
            FROM alarm_history
            {whereClause};
            """;

        var totalCount = Convert.ToInt32(
            await countCommand.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);

        if (totalCount == 0)
        {
            return new AlarmHistoryPageResult();
        }

        var command = connection.CreateCommand();
        whereClause = BuildWhereClause(query, command);

        command.CommandText =
            $"""
            SELECT id,
                   alarm_key,
                   tag_name,
                   address,
                   title,
                   description,
                   remedy,
                   alarm_type,
                   severity,
                   status,
                   stop_machine,
                   code_value,
                   raw_value,
                   machine_code,
                   started_at_utc,
                   ended_at_utc,
                   duration_seconds,
                   created_at_utc,
                   updated_at_utc
            FROM alarm_history
            {whereClause}
            ORDER BY started_at_utc DESC, id DESC
            LIMIT $limit OFFSET $offset;
            """;

        command.Parameters.AddWithValue("$limit", pageSize);
        command.Parameters.AddWithValue("$offset", offset);

        var items = new List<AlarmHistoryItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(
                new AlarmHistoryItem
                {
                    Id = reader.GetInt64(0),
                    AlarmKey = reader.GetString(1),
                    TagName = reader.GetString(2),
                    Address = reader.GetString(3),
                    Title = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Description = AlarmDefinitionCatalog.NormalizeDescription(reader.IsDBNull(5) ? null : reader.GetString(5)),
                    Remedy = AlarmDefinitionCatalog.NormalizeRemedy(reader.IsDBNull(6) ? null : reader.GetString(6)),
                    AlarmType = Enum.Parse<AlarmType>(reader.GetString(7), true),
                    Severity = Enum.Parse<AlarmSeverity>(reader.GetString(8), true),
                    Status = Enum.Parse<AlarmRecordStatus>(reader.GetString(9), true),
                    StopMachine = reader.GetInt32(10) == 1,
                    CodeValue = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    RawValue = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    MachineCode = reader.GetString(13),
                    StartedAtUtc = ParseUtc(reader.GetString(14)),
                    EndedAtUtc = reader.IsDBNull(15) ? null : ParseUtc(reader.GetString(15)),
                    DurationSeconds = reader.IsDBNull(16) ? null : reader.GetInt32(16),
                    CreatedAtUtc = ParseUtc(reader.GetString(17)),
                    UpdatedAtUtc = ParseUtc(reader.GetString(18)),
                });
        }

        return new AlarmHistoryPageResult
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    public async Task<AlarmHistorySummary> GetSummaryAsync(DateTime localNow, CancellationToken cancellationToken = default)
    {
        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var localStart = DateTime.SpecifyKind(localNow.Date, DateTimeKind.Local);
        var localEnd = DateTime.SpecifyKind(localNow.Date.AddDays(1), DateTimeKind.Local);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                COALESCE(SUM(CASE WHEN status = $activeStatus THEN 1 ELSE 0 END), 0),
                COALESCE(SUM(CASE WHEN status = $activeStatus AND stop_machine = 1 THEN 1 ELSE 0 END), 0),
                COALESCE(SUM(CASE WHEN started_at_utc >= $todayStartUtc AND started_at_utc < $todayEndUtc THEN 1 ELSE 0 END), 0)
            FROM alarm_history;
            """;

        command.Parameters.AddWithValue("$activeStatus", AlarmRecordStatus.Active.ToString());
        command.Parameters.AddWithValue("$todayStartUtc", ToUtcText(localStart.ToUniversalTime()));
        command.Parameters.AddWithValue("$todayEndUtc", ToUtcText(localEnd.ToUniversalTime()));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new AlarmHistorySummary();
        }

        return new AlarmHistorySummary
        {
            ActiveCount = reader.GetInt32(0),
            StopMachineActiveCount = reader.GetInt32(1),
            TodayCount = reader.GetInt32(2),
        };
    }

    private static string ToUtcText(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value.ToString("O") : value.ToUniversalTime().ToString("O");
    }

    private static DateTime ParseUtc(string value)
    {
        return DateTimeOffset.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind).UtcDateTime;
    }

    private static string BuildWhereClause(AlarmHistoryQuery query, SqliteCommand command)
    {
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.SearchTag))
        {
            filters.Add("(tag_name LIKE $search OR address LIKE $search OR title LIKE $search OR description LIKE $search OR remedy LIKE $search)");
            command.Parameters.AddWithValue("$search", $"%{query.SearchTag.Trim()}%");
        }

        if (query.Status is not null)
        {
            filters.Add("status = $status");
            command.Parameters.AddWithValue("$status", query.Status.Value.ToString());
        }

        if (query.Type is not null)
        {
            filters.Add("alarm_type = $type");
            command.Parameters.AddWithValue("$type", query.Type.Value.ToString());
        }

        if (query.FromLocalDate is not null)
        {
            var fromUtc = DateTime.SpecifyKind(query.FromLocalDate.Value.Date, DateTimeKind.Local).ToUniversalTime();
            filters.Add("started_at_utc >= $fromUtc");
            command.Parameters.AddWithValue("$fromUtc", ToUtcText(fromUtc));
        }

        if (query.ToLocalDate is not null)
        {
            var nextLocalDate = DateTime.SpecifyKind(query.ToLocalDate.Value.Date.AddDays(1), DateTimeKind.Local);
            filters.Add("started_at_utc < $toUtcExclusive");
            command.Parameters.AddWithValue("$toUtcExclusive", ToUtcText(nextLocalDate.ToUniversalTime()));
        }

        return filters.Count > 0 ? $"WHERE {string.Join(" AND ", filters)}" : string.Empty;
    }

    public async Task<int> DeleteByFilterAsync(AlarmHistoryQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        var whereClause = BuildWhereClause(query, command);
        command.CommandText = $"DELETE FROM alarm_history {whereClause};";

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
