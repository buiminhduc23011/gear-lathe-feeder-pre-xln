using Desktop.App.Configuration.Plc;
using Desktop.App.Data.Repositories;
using Desktop.App.Models.Ui;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Plc;

namespace Desktop.App.Services;

public sealed class PlcParameterSettingsService : IPlcParameterSettingsService
{
    private readonly PlcParameterSettingsRepository _repository;

    public PlcParameterSettingsService(PlcParameterSettingsRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IReadOnlyList<EditablePlcParameterField>> LoadGroupAsync(string groupName, CancellationToken cancellationToken = default)
    {
        var tags = PlcParameterGroups.GetTags(groupName);
        var storedRecords = await _repository.GetByGroupAsync(groupName, cancellationToken);
        var recordLookup = storedRecords.ToDictionary(record => record.TagName, StringComparer.OrdinalIgnoreCase);
        var fields = new List<EditablePlcParameterField>(tags.Count);

        foreach (var tag in tags)
        {
            var valueText = recordLookup.TryGetValue(tag.Name, out var record)
                ? record.ValueText
                : PlcTagValueTextConverter.Format(tag, PlcTagValueTextConverter.CreateDefaultValue(tag));

            fields.Add(new EditablePlcParameterField(tag, groupName, valueText));
        }

        return fields;
    }

    public async Task<IReadOnlyDictionary<string, object>> LoadDesiredSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var records = await _repository.GetAllAsync(cancellationToken);
        var snapshot = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var record in records)
        {
            if (!PlcTagCatalog.TryGet(record.TagName, out var definition))
            {
                continue;
            }

            if (!PlcTagValueTextConverter.TryParse(definition, record.ValueText, out var typedValue, out _))
            {
                continue;
            }

            snapshot[record.TagName] = typedValue;
        }

        return snapshot;
    }

    public async Task UpsertAsync(string tagName, object typedValue, CancellationToken cancellationToken = default)
    {
        if (!PlcTagCatalog.TryGet(tagName, out var definition))
        {
            throw new ArgumentException($"Unknown PLC tag '{tagName}'.", nameof(tagName));
        }

        if (!PlcParameterGroups.TryGetGroupName(tagName, out var groupName))
        {
            throw new ArgumentException($"PLC tag '{tagName}' does not belong to a supported parameter group.", nameof(tagName));
        }

        var valueText = PlcTagValueTextConverter.Format(definition, typedValue);
        await _repository.UpsertAsync(tagName, groupName, valueText, cancellationToken);
    }
}
