using System.Collections.ObjectModel;

namespace Desktop.App.Models.Runtime;

public sealed class PlcParameterSyncChangedEventArgs : EventArgs
{
    public PlcParameterSyncChangedEventArgs(IReadOnlyDictionary<string, bool> syncStates)
    {
        SyncStates = new ReadOnlyDictionary<string, bool>(new Dictionary<string, bool>(syncStates, StringComparer.OrdinalIgnoreCase));
    }

    public IReadOnlyDictionary<string, bool> SyncStates { get; }
}
