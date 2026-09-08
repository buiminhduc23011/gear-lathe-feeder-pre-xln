using Desktop.App.Models.Ui;

namespace Desktop.App.Services.Abstractions;

public interface IPlcParameterSettingsService
{
    Task<IReadOnlyList<EditablePlcParameterField>> LoadGroupAsync(string groupName, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, object>> LoadDesiredSnapshotAsync(CancellationToken cancellationToken = default);

    Task UpsertAsync(string tagName, object typedValue, CancellationToken cancellationToken = default);
}
