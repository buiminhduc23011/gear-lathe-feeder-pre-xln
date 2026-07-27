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
                   plc_line1_host,
                   plc_line1_port,
                   plc_line1_slave_id,
                   plc_line1_connection_mode,
                   plc_line1_poll_interval_ms,
                   plc_line2_host,
                   plc_line2_port,
                   plc_line2_slave_id,
                   plc_line2_connection_mode,
                   plc_line2_poll_interval_ms,
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
            PlcLine1Host = reader.IsDBNull(9) ? "127.0.0.1" : reader.GetString(9),
            PlcLine1Port = reader.IsDBNull(10) ? 502 : reader.GetInt32(10),
            PlcLine1SlaveId = reader.IsDBNull(11) ? 1 : reader.GetInt32(11),
            PlcLine1ConnectionMode = reader.IsDBNull(12) ? "DVP" : reader.GetString(12),
            PlcLine1PollIntervalMs = reader.IsDBNull(13) ? 100 : reader.GetInt32(13),
            PlcLine2Host = reader.IsDBNull(14) ? "127.0.0.1" : reader.GetString(14),
            PlcLine2Port = reader.IsDBNull(15) ? 502 : reader.GetInt32(15),
            PlcLine2SlaveId = reader.IsDBNull(16) ? 1 : reader.GetInt32(16),
            PlcLine2ConnectionMode = reader.IsDBNull(17) ? "DVP" : reader.GetString(17),
            PlcLine2PollIntervalMs = reader.IsDBNull(18) ? 100 : reader.GetInt32(18),
            VirtualKeyboardEnabled = !reader.IsDBNull(19) && reader.GetInt32(19) != 0,
            AgvBaseUrl = reader.IsDBNull(20) ? string.Empty : reader.GetString(20),
            AgvAutoCallEnabled = !reader.IsDBNull(21) && reader.GetInt32(21) != 0,
            AgvKe1AutoCallRemainingBelow = reader.IsDBNull(22) ? 5 : reader.GetInt32(22),
            AgvKe2AutoCallRemainingBelow = reader.IsDBNull(23) ? 5 : reader.GetInt32(23),
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
                plc_line1_host,
                plc_line1_port,
                plc_line1_slave_id,
                plc_line1_connection_mode,
                plc_line1_poll_interval_ms,
                plc_line2_host,
                plc_line2_port,
                plc_line2_slave_id,
                plc_line2_connection_mode,
                plc_line2_poll_interval_ms,
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
                $plcLine1Host,
                $plcLine1Port,
                $plcLine1SlaveId,
                $plcLine1ConnectionMode,
                $plcLine1PollIntervalMs,
                $plcLine2Host,
                $plcLine2Port,
                $plcLine2SlaveId,
                $plcLine2ConnectionMode,
                $plcLine2PollIntervalMs,
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
                plc_line1_host = excluded.plc_line1_host,
                plc_line1_port = excluded.plc_line1_port,
                plc_line1_slave_id = excluded.plc_line1_slave_id,
                plc_line1_connection_mode = excluded.plc_line1_connection_mode,
                plc_line1_poll_interval_ms = excluded.plc_line1_poll_interval_ms,
                plc_line2_host = excluded.plc_line2_host,
                plc_line2_port = excluded.plc_line2_port,
                plc_line2_slave_id = excluded.plc_line2_slave_id,
                plc_line2_connection_mode = excluded.plc_line2_connection_mode,
                plc_line2_poll_interval_ms = excluded.plc_line2_poll_interval_ms,
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
        command.Parameters.AddWithValue("$plcLine1Host", options.PlcLine1Host.Trim());
        command.Parameters.AddWithValue("$plcLine1Port", options.PlcLine1Port);
        command.Parameters.AddWithValue("$plcLine1SlaveId", options.PlcLine1SlaveId);
        command.Parameters.AddWithValue("$plcLine1ConnectionMode", options.PlcLine1ConnectionMode.Trim());
        command.Parameters.AddWithValue("$plcLine1PollIntervalMs", options.PlcLine1PollIntervalMs);
        command.Parameters.AddWithValue("$plcLine2Host", options.PlcLine2Host.Trim());
        command.Parameters.AddWithValue("$plcLine2Port", options.PlcLine2Port);
        command.Parameters.AddWithValue("$plcLine2SlaveId", options.PlcLine2SlaveId);
        command.Parameters.AddWithValue("$plcLine2ConnectionMode", options.PlcLine2ConnectionMode.Trim());
        command.Parameters.AddWithValue("$plcLine2PollIntervalMs", options.PlcLine2PollIntervalMs);
        command.Parameters.AddWithValue("$virtualKeyboardEnabled", options.VirtualKeyboardEnabled ? 1 : 0);
        command.Parameters.AddWithValue("$agvBaseUrl", options.AgvBaseUrl.Trim());
        command.Parameters.AddWithValue("$agvAutoCallEnabled", options.AgvAutoCallEnabled ? 1 : 0);
        command.Parameters.AddWithValue("$agvKe1AutoCallRemainingBelow", options.AgvKe1AutoCallRemainingBelow);
        command.Parameters.AddWithValue("$agvKe2AutoCallRemainingBelow", options.AgvKe2AutoCallRemainingBelow);
        command.Parameters.AddWithValue("$updatedAtUtc", DateTimeOffset.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
