using Desktop.App.Configuration;
using Desktop.App.Data.Sqlite;
using Microsoft.Data.Sqlite;

namespace Desktop.App.Data.Repositories;

public sealed class SettingsRepository
{
    private const int SingletonRecordId = 1;
    private readonly string? _databasePath;

    public SettingsRepository(string? databasePath = null)
    {
        _databasePath = databasePath;
    }

    public async Task<AppOptions> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT machine_name,
                   machine_code,
                   description,
                   plc_host,
                   plc_port,
                   plc_connection_mode,
                   plc_slave_id,
                   poll_interval_ms,
                   api_base_url,
                   virtual_keyboard_enabled,
                   agv_base_url,
                   agv_auto_call_enabled,
                   agv_ke1_auto_call_remaining_below,
                   agv_ke2_auto_call_remaining_below
            FROM machine_settings
            WHERE id = $id
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$id", SingletonRecordId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new AppOptions();
        }

        return new AppOptions
        {
            MachineName = reader.GetString(0),
            MachineCode = reader.GetString(1),
            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            PlcHost = reader.GetString(3),
            PlcPort = reader.GetInt32(4),
            PlcConnectionMode = reader.GetString(5),
            PlcSlaveId = reader.GetInt32(6),
            PollIntervalMs = reader.GetInt32(7),
            ApiBaseUrl = reader.GetString(8),
            VirtualKeyboardEnabled = !reader.IsDBNull(9) && reader.GetInt32(9) != 0,
            AgvBaseUrl = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
            AgvAutoCallEnabled = !reader.IsDBNull(11) && reader.GetInt32(11) != 0,
            AgvKe1AutoCallRemainingBelow = reader.IsDBNull(12) ? 5 : reader.GetInt32(12),
            AgvKe2AutoCallRemainingBelow = reader.IsDBNull(13) ? 5 : reader.GetInt32(13),
        };
    }

    public async Task SaveAsync(AppOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        await Migrations.ApplyAsync(_databasePath, cancellationToken);

        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO machine_settings
            (
                id,
                machine_name,
                machine_code,
                description,
                plc_host,
                plc_port,
                plc_connection_mode,
                plc_slave_id,
                poll_interval_ms,
                api_base_url,
                virtual_keyboard_enabled,
                agv_base_url,
                agv_auto_call_enabled,
                agv_ke1_auto_call_remaining_below,
                agv_ke2_auto_call_remaining_below,
                updated_at_utc
            )
            VALUES
            (
                $id,
                $machineName,
                $machineCode,
                $description,
                $plcHost,
                $plcPort,
                $plcConnectionMode,
                $plcSlaveId,
                $pollIntervalMs,
                $apiBaseUrl,
                $virtualKeyboardEnabled,
                $agvBaseUrl,
                $agvAutoCallEnabled,
                $agvKe1AutoCallRemainingBelow,
                $agvKe2AutoCallRemainingBelow,
                $updatedAtUtc
            )
            ON CONFLICT(id) DO UPDATE SET
                machine_name = excluded.machine_name,
                machine_code = excluded.machine_code,
                description = excluded.description,
                plc_host = excluded.plc_host,
                plc_port = excluded.plc_port,
                plc_connection_mode = excluded.plc_connection_mode,
                plc_slave_id = excluded.plc_slave_id,
                poll_interval_ms = excluded.poll_interval_ms,
                api_base_url = excluded.api_base_url,
                virtual_keyboard_enabled = excluded.virtual_keyboard_enabled,
                agv_base_url = excluded.agv_base_url,
                agv_auto_call_enabled = excluded.agv_auto_call_enabled,
                agv_ke1_auto_call_remaining_below = excluded.agv_ke1_auto_call_remaining_below,
                agv_ke2_auto_call_remaining_below = excluded.agv_ke2_auto_call_remaining_below,
                updated_at_utc = excluded.updated_at_utc;
            """;

        command.Parameters.AddWithValue("$id", SingletonRecordId);
        command.Parameters.AddWithValue("$machineName", options.MachineName.Trim());
        command.Parameters.AddWithValue("$machineCode", options.MachineCode.Trim());
        command.Parameters.AddWithValue("$description", string.IsNullOrWhiteSpace(options.Description) ? DBNull.Value : options.Description.Trim());
        command.Parameters.AddWithValue("$plcHost", options.PlcHost.Trim());
        command.Parameters.AddWithValue("$plcPort", options.PlcPort);
        command.Parameters.AddWithValue("$plcConnectionMode", options.PlcConnectionMode.Trim());
        command.Parameters.AddWithValue("$plcSlaveId", options.PlcSlaveId);
        command.Parameters.AddWithValue("$pollIntervalMs", options.PollIntervalMs);
        command.Parameters.AddWithValue("$apiBaseUrl", options.ApiBaseUrl.Trim());
        command.Parameters.AddWithValue("$virtualKeyboardEnabled", options.VirtualKeyboardEnabled ? 1 : 0);
        command.Parameters.AddWithValue("$agvBaseUrl", options.AgvBaseUrl.Trim());
        command.Parameters.AddWithValue("$agvAutoCallEnabled", options.AgvAutoCallEnabled ? 1 : 0);
        command.Parameters.AddWithValue("$agvKe1AutoCallRemainingBelow", options.AgvKe1AutoCallRemainingBelow);
        command.Parameters.AddWithValue("$agvKe2AutoCallRemainingBelow", options.AgvKe2AutoCallRemainingBelow);
        command.Parameters.AddWithValue("$updatedAtUtc", DateTimeOffset.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
