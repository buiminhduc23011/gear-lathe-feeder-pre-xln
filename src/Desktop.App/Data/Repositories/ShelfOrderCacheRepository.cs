using Desktop.App.Data.Sqlite;
using Desktop.App.Models.Tray;
using Microsoft.Data.Sqlite;

namespace Desktop.App.Data.Repositories;

public interface IShelfOrderCacheRepository
{
    Task<ShelfOrderCache?> GetAsync(int keIndex, CancellationToken cancellationToken = default);
    Task SaveAsync(ShelfOrderCache cache, CancellationToken cancellationToken = default);
    Task DeleteAsync(int keIndex, CancellationToken cancellationToken = default);
}

public sealed class ShelfOrderCacheRepository : IShelfOrderCacheRepository
{
    private readonly string? _databasePath;

    public ShelfOrderCacheRepository(string? databasePath = null)
    {
        _databasePath = databasePath;
    }

    public async Task<ShelfOrderCache?> GetAsync(int keIndex, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, ke_index, declaration_id, current_order_sequence, total_orders, shelf_layout_type,
                   current_item_in_order, awaiting_pick_reset, completion_reported, completion_acknowledged,
                   order_qty_snapshot, ran_qty_snapshot, orders_json, updated_at_utc
            FROM shelf_order_cache
            WHERE ke_index = $keIndex
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$keIndex", keIndex);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new ShelfOrderCache
            {
                Id = reader.GetInt32(0),
                KeIndex = reader.GetInt32(1),
                DeclarationId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                CurrentOrderSequence = reader.GetInt32(3),
                TotalOrders = reader.GetInt32(4),
                ShelfLayoutType = reader.GetInt32(5),
                CurrentItemInOrder = reader.GetInt32(6),
                AwaitingPickReset = reader.GetInt32(7) != 0,
                CompletionReported = reader.GetInt32(8) != 0,
                CompletionAcknowledged = reader.GetInt32(9) != 0,
                OrderQtySnapshot = reader.GetInt32(10),
                RanQtySnapshot = reader.GetInt32(11),
                OrdersJson = reader.IsDBNull(12) ? "[]" : reader.GetString(12),
                UpdatedAtUtc = DateTime.Parse(reader.GetString(13)).ToUniversalTime()
            };
        }

        return null;
    }

    public async Task SaveAsync(ShelfOrderCache cache, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO shelf_order_cache (
                ke_index,
                declaration_id,
                current_order_sequence,
                total_orders,
                shelf_layout_type,
                current_item_in_order,
                awaiting_pick_reset,
                completion_reported,
                completion_acknowledged,
                order_qty_snapshot,
                ran_qty_snapshot,
                orders_json,
                updated_at_utc)
            VALUES (
                $keIndex,
                $declarationId,
                $currentOrderSequence,
                $totalOrders,
                $shelfLayoutType,
                $currentItemInOrder,
                $awaitingPickReset,
                $completionReported,
                $completionAcknowledged,
                $orderQtySnapshot,
                $ranQtySnapshot,
                $ordersJson,
                $updatedAtUtc)
            ON CONFLICT(ke_index) DO UPDATE SET
                declaration_id = excluded.declaration_id,
                current_order_sequence = excluded.current_order_sequence,
                total_orders = excluded.total_orders,
                shelf_layout_type = excluded.shelf_layout_type,
                current_item_in_order = excluded.current_item_in_order,
                awaiting_pick_reset = excluded.awaiting_pick_reset,
                completion_reported = excluded.completion_reported,
                completion_acknowledged = excluded.completion_acknowledged,
                order_qty_snapshot = excluded.order_qty_snapshot,
                ran_qty_snapshot = excluded.ran_qty_snapshot,
                orders_json = excluded.orders_json,
                updated_at_utc = excluded.updated_at_utc;
            """;

        command.Parameters.AddWithValue("$keIndex", cache.KeIndex);
        command.Parameters.AddWithValue("$declarationId", (object?)cache.DeclarationId ?? DBNull.Value);
        command.Parameters.AddWithValue("$currentOrderSequence", cache.CurrentOrderSequence);
        command.Parameters.AddWithValue("$totalOrders", cache.TotalOrders);
        command.Parameters.AddWithValue("$shelfLayoutType", cache.ShelfLayoutType);
        command.Parameters.AddWithValue("$currentItemInOrder", cache.CurrentItemInOrder);
        command.Parameters.AddWithValue("$awaitingPickReset", cache.AwaitingPickReset ? 1 : 0);
        command.Parameters.AddWithValue("$completionReported", cache.CompletionReported ? 1 : 0);
        command.Parameters.AddWithValue("$completionAcknowledged", cache.CompletionAcknowledged ? 1 : 0);
        command.Parameters.AddWithValue("$orderQtySnapshot", cache.OrderQtySnapshot);
        command.Parameters.AddWithValue("$ranQtySnapshot", cache.RanQtySnapshot);
        command.Parameters.AddWithValue("$ordersJson", cache.OrdersJson);
        command.Parameters.AddWithValue("$updatedAtUtc", DateTimeOffset.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(int keIndex, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(AppDb.GetConnectionString(_databasePath));
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM shelf_order_cache WHERE ke_index = $keIndex;";
        command.Parameters.AddWithValue("$keIndex", keIndex);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
