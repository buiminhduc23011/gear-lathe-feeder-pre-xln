using Desktop.App.Configuration.Plc;
using Microsoft.Data.Sqlite;

namespace Desktop.App.Data.Sqlite;

public static class Migrations
{
    public static async Task ApplyAsync(string? databasePath = null, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS machine_settings
            (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                machine_name TEXT NOT NULL,
                machine_code TEXT NOT NULL,
                description TEXT NULL,
                plc_host TEXT NOT NULL,
                plc_port INTEGER NOT NULL,
                plc_connection_mode TEXT NOT NULL DEFAULT 'DVP',
                plc_slave_id INTEGER NOT NULL DEFAULT 1,
                poll_interval_ms INTEGER NOT NULL DEFAULT 100,
                api_base_url TEXT NOT NULL,
                updated_at_utc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS alarm_history
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                alarm_key TEXT NOT NULL,
                tag_name TEXT NOT NULL,
                address TEXT NOT NULL,
                code_value INTEGER NULL,
                title TEXT NOT NULL,
                description TEXT NOT NULL,
                remedy TEXT NOT NULL,
                alarm_type TEXT NOT NULL,
                severity TEXT NOT NULL,
                status TEXT NOT NULL,
                stop_machine INTEGER NOT NULL,
                started_at_utc TEXT NOT NULL,
                ended_at_utc TEXT NULL,
                duration_seconds INTEGER NULL,
                machine_code TEXT NOT NULL,
                raw_value TEXT NULL,
                created_at_utc TEXT NOT NULL,
                updated_at_utc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_alarm_history_status_started
                ON alarm_history(status, started_at_utc DESC);

            CREATE INDEX IF NOT EXISTS idx_alarm_history_tag_started
                ON alarm_history(tag_name, started_at_utc DESC);

            CREATE INDEX IF NOT EXISTS idx_alarm_history_type_started
                ON alarm_history(alarm_type, started_at_utc DESC);

            CREATE TABLE IF NOT EXISTS plc_parameter_settings
            (
                tag_name TEXT PRIMARY KEY,
                group_name TEXT NOT NULL,
                value_text TEXT NOT NULL,
                updated_at_utc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_plc_parameter_settings_group
                ON plc_parameter_settings(group_name, tag_name);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
        await EnsureDefaultPlcParameterSettingsAsync(connection, cancellationToken);
        await EnsurePlcConnectionModeColumnAsync(connection, cancellationToken);
        await EnsurePlcSlaveIdColumnAsync(connection, cancellationToken);
        await EnsurePollIntervalColumnAsync(connection, cancellationToken);
        await EnsurePlcLine1ColumnsAsync(connection, cancellationToken);
        await EnsurePlcLine2ColumnsAsync(connection, cancellationToken);
        await EnsureVirtualKeyboardColumnAsync(connection, cancellationToken);
        await EnsureAgvSettingsColumnsAsync(connection, cancellationToken);
        await EnsureAgvCallHistoryTableAsync(connection, cancellationToken);
        await EnsureTrayConfigsTableAsync(connection, cancellationToken);
        await EnsureShelfOrderCacheTableAsync(connection, cancellationToken);
        await EnsureAuditLogTableAsync(connection, cancellationToken);
    }

    private static async Task EnsureDefaultPlcParameterSettingsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var updatedAtUtc = DateTimeOffset.UtcNow.ToString("O");
        (string TagName, string GroupName, string ValueText)[] defaultSettings = [];

        foreach (var setting in defaultSettings)
        {
            var command = connection.CreateCommand();
            command.CommandText =
                """
                INSERT OR IGNORE INTO plc_parameter_settings
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
                );
                """;
            command.Parameters.AddWithValue("$tagName", setting.TagName);
            command.Parameters.AddWithValue("$groupName", setting.GroupName);
            command.Parameters.AddWithValue("$valueText", setting.ValueText);
            command.Parameters.AddWithValue("$updatedAtUtc", updatedAtUtc);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsurePlcConnectionModeColumnAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA table_info(machine_settings);";

        var hasPlcConnectionModeColumn = false;
        await using (var reader = await pragmaCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                if (string.Equals(reader.GetString(1), "plc_connection_mode", StringComparison.OrdinalIgnoreCase))
                {
                    hasPlcConnectionModeColumn = true;
                    break;
                }
            }
        }

        if (hasPlcConnectionModeColumn)
        {
            return;
        }

        var alterCommand = connection.CreateCommand();
        alterCommand.CommandText =
            """
            ALTER TABLE machine_settings
            ADD COLUMN plc_connection_mode TEXT NOT NULL DEFAULT 'DVP';
            """;

        await alterCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsurePlcSlaveIdColumnAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA table_info(machine_settings);";

        var hasPlcSlaveIdColumn = false;
        await using (var reader = await pragmaCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                if (string.Equals(reader.GetString(1), "plc_slave_id", StringComparison.OrdinalIgnoreCase))
                {
                    hasPlcSlaveIdColumn = true;
                    break;
                }
            }
        }

        if (hasPlcSlaveIdColumn)
        {
            return;
        }

        var alterCommand = connection.CreateCommand();
        alterCommand.CommandText =
            """
            ALTER TABLE machine_settings
            ADD COLUMN plc_slave_id INTEGER NOT NULL DEFAULT 1;
            """;

        await alterCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsurePollIntervalColumnAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA table_info(machine_settings);";

        var hasPollIntervalColumn = false;
        await using (var reader = await pragmaCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                if (string.Equals(reader.GetString(1), "poll_interval_ms", StringComparison.OrdinalIgnoreCase))
                {
                    hasPollIntervalColumn = true;
                    break;
                }
            }
        }

        if (hasPollIntervalColumn)
        {
            return;
        }

        var alterCommand = connection.CreateCommand();
        alterCommand.CommandText =
            """
            ALTER TABLE machine_settings
            ADD COLUMN poll_interval_ms INTEGER NOT NULL DEFAULT 100;
            """;

        await alterCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsurePlcLine1ColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var existingColumns = await GetExistingColumnsAsync(connection, cancellationToken);

        string[][] columnsToAdd =
        [
            ["plc_line1_host", "TEXT NOT NULL DEFAULT '127.0.0.1'"],
            ["plc_line1_port", "INTEGER NOT NULL DEFAULT 502"],
            ["plc_line1_slave_id", "INTEGER NOT NULL DEFAULT 1"],
            ["plc_line1_connection_mode", "TEXT NOT NULL DEFAULT 'DVP'"],
            ["plc_line1_poll_interval_ms", "INTEGER NOT NULL DEFAULT 100"],
        ];

        foreach (var col in columnsToAdd)
        {
            if (existingColumns.Contains(col[0]))
            {
                continue;
            }

            var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE machine_settings ADD COLUMN {col[0]} {col[1]};";
            await alterCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsurePlcLine2ColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var existingColumns = await GetExistingColumnsAsync(connection, cancellationToken);

        string[][] columnsToAdd =
        [
            ["plc_line2_host", "TEXT NOT NULL DEFAULT '127.0.0.1'"],
            ["plc_line2_port", "INTEGER NOT NULL DEFAULT 502"],
            ["plc_line2_slave_id", "INTEGER NOT NULL DEFAULT 1"],
            ["plc_line2_connection_mode", "TEXT NOT NULL DEFAULT 'DVP'"],
            ["plc_line2_poll_interval_ms", "INTEGER NOT NULL DEFAULT 100"],
        ];

        foreach (var col in columnsToAdd)
        {
            if (existingColumns.Contains(col[0]))
            {
                continue;
            }

            var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE machine_settings ADD COLUMN {col[0]} {col[1]};";
            await alterCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureVirtualKeyboardColumnAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var existingColumns = await GetExistingColumnsAsync(connection, cancellationToken);
        if (!existingColumns.Contains("virtual_keyboard_enabled"))
        {
            var alterCommand = connection.CreateCommand();
            alterCommand.CommandText =
                "ALTER TABLE machine_settings ADD COLUMN virtual_keyboard_enabled INTEGER NOT NULL DEFAULT 1;";
            await alterCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureAgvSettingsColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var existingColumns = await GetExistingColumnsAsync(connection, cancellationToken);

        string[][] columnsToAdd =
        [
            ["agv_base_url", "TEXT NOT NULL DEFAULT ''"],
            ["agv_mac_address", "TEXT NOT NULL DEFAULT ''"],
            ["agv_auto_call_enabled", "INTEGER NOT NULL DEFAULT 0"],
            ["agv_ke1_auto_call_remaining_below", "INTEGER NOT NULL DEFAULT 5"],
            ["agv_ke2_auto_call_remaining_below", "INTEGER NOT NULL DEFAULT 5"],
        ];

        foreach (var col in columnsToAdd)
        {
            if (existingColumns.Contains(col[0]))
            {
                continue;
            }

            var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE machine_settings ADD COLUMN {col[0]} {col[1]};";
            await alterCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureAgvCallHistoryTableAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS agv_call_history
            (
                id               INTEGER PRIMARY KEY AUTOINCREMENT,
                ke_type          INTEGER NOT NULL,
                status           INTEGER NOT NULL,
                is_auto_call     INTEGER NOT NULL DEFAULT 0,
                created_at_utc   TEXT NOT NULL,
                started_at_utc   TEXT NULL,
                completed_at_utc TEXT NULL,
                note             TEXT NULL,
                remaining_qty    INTEGER NOT NULL DEFAULT 0
            );

            CREATE INDEX IF NOT EXISTS idx_agv_call_history_created
                ON agv_call_history(created_at_utc DESC);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<HashSet<string>> GetExistingColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA table_info(machine_settings);";

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await pragmaCommand.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static async Task EnsureTrayConfigsTableAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS tray_configs
            (
                id             INTEGER PRIMARY KEY AUTOINCREMENT,
                tray_size      INTEGER NOT NULL UNIQUE,
                rows           INTEGER NOT NULL DEFAULT 5,
                columns        INTEGER NOT NULL DEFAULT 9,
                row_offset     REAL NOT NULL DEFAULT 0,
                col_offset     REAL NOT NULL DEFAULT 0,
                updated_at_utc TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);

        // Seed 2 default records (Small + Large) if empty
        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM tray_configs;";
        var count = Convert.ToInt64(await countCommand.ExecuteScalarAsync(cancellationToken));

        if (count == 0)
        {
            var seedCommand = connection.CreateCommand();
            seedCommand.CommandText =
                """
                INSERT INTO tray_configs (tray_size, rows, columns, row_offset, col_offset, updated_at_utc)
                VALUES
                    (1, 5, 9, 55.0, 55.0, $now),
                    (2, 4, 8, 68.0, 68.0, $now);
                """;
            seedCommand.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
            await seedCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureShelfOrderCacheTableAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS shelf_order_cache
            (
                id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                ke_index                INTEGER NOT NULL UNIQUE,
                declaration_id          INTEGER NULL,
                current_order_sequence  INTEGER NOT NULL DEFAULT 0,
                total_orders            INTEGER NOT NULL DEFAULT 0,
                shelf_layout_type       INTEGER NOT NULL DEFAULT 0,
                current_item_in_order   INTEGER NOT NULL DEFAULT 0,
                awaiting_pick_reset     INTEGER NOT NULL DEFAULT 0,
                completion_reported     INTEGER NOT NULL DEFAULT 0,
                completion_acknowledged INTEGER NOT NULL DEFAULT 0,
                order_qty_snapshot      INTEGER NOT NULL DEFAULT 0,
                ran_qty_snapshot        INTEGER NOT NULL DEFAULT 0,
                orders_json             TEXT NOT NULL DEFAULT '[]',
                updated_at_utc          TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);

        // Ensure shelf_layout_type column exists (added later)
        var pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA table_info(shelf_order_cache);";
        var hasLayoutCol = false;
        await using (var reader = await pragmaCmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                if (string.Equals(reader.GetString(1), "shelf_layout_type", StringComparison.OrdinalIgnoreCase))
                {
                    hasLayoutCol = true;
                    break;
                }
            }
        }

        if (!hasLayoutCol)
        {
            var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE shelf_order_cache ADD COLUMN shelf_layout_type INTEGER NOT NULL DEFAULT 0;";
            await alterCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        var pragmaDeclarationCmd = connection.CreateCommand();
        pragmaDeclarationCmd.CommandText = "PRAGMA table_info(shelf_order_cache);";
        var hasDeclarationIdCol = false;
        await using (var reader = await pragmaDeclarationCmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                if (string.Equals(reader.GetString(1), "declaration_id", StringComparison.OrdinalIgnoreCase))
                {
                    hasDeclarationIdCol = true;
                    break;
                }
            }
        }

        if (!hasDeclarationIdCol)
        {
            var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE shelf_order_cache ADD COLUMN declaration_id INTEGER NULL;";
            await alterCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await EnsureShelfOrderCacheColumnAsync(connection, "current_item_in_order", "INTEGER NOT NULL DEFAULT 0", cancellationToken);
        await EnsureShelfOrderCacheColumnAsync(connection, "awaiting_pick_reset", "INTEGER NOT NULL DEFAULT 0", cancellationToken);
        await EnsureShelfOrderCacheColumnAsync(connection, "completion_reported", "INTEGER NOT NULL DEFAULT 0", cancellationToken);
        await EnsureShelfOrderCacheColumnAsync(connection, "completion_acknowledged", "INTEGER NOT NULL DEFAULT 0", cancellationToken);
        await EnsureShelfOrderCacheColumnAsync(connection, "order_qty_snapshot", "INTEGER NOT NULL DEFAULT 0", cancellationToken);
        await EnsureShelfOrderCacheColumnAsync(connection, "ran_qty_snapshot", "INTEGER NOT NULL DEFAULT 0", cancellationToken);
    }

    private static async Task EnsureShelfOrderCacheColumnAsync(
        SqliteConnection connection,
        string columnName,
        string definition,
        CancellationToken cancellationToken)
    {
        var pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA table_info(shelf_order_cache);";
        var hasColumn = false;
        await using (var reader = await pragmaCmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    hasColumn = true;
                    break;
                }
            }
        }

        if (!hasColumn)
        {
            var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = $"ALTER TABLE shelf_order_cache ADD COLUMN {columnName} {definition};";
            await alterCmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureAuditLogTableAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS audit_log
            (
                id             INTEGER PRIMARY KEY AUTOINCREMENT,
                action         TEXT NOT NULL,
                username       TEXT NOT NULL,
                detail         TEXT NULL,
                created_at_utc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_audit_log_created
                ON audit_log(created_at_utc DESC);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
