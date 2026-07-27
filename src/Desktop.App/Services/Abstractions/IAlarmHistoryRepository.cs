using Desktop.App.Models.Alarms;

namespace Desktop.App.Services.Abstractions;

public interface IAlarmHistoryRepository
{
    Task<long> InsertActiveAsync(AlarmHistoryItem item, CancellationToken cancellationToken = default);

    Task ResolveAsync(string alarmKey, DateTime endedAtUtc, CancellationToken cancellationToken = default);

    Task ResolveAllActiveAsync(DateTime endedAtUtc, CancellationToken cancellationToken = default);

    Task<AlarmHistoryPageResult> QueryAsync(AlarmHistoryQuery query, CancellationToken cancellationToken = default);

    Task<AlarmHistorySummary> GetSummaryAsync(DateTime localNow, CancellationToken cancellationToken = default);

    Task<int> DeleteByFilterAsync(AlarmHistoryQuery query, CancellationToken cancellationToken = default);
}
