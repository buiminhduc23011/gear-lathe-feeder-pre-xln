using System.Collections.ObjectModel;

namespace Desktop.App.Models.Runtime;

public sealed class PlcDataChangedEventArgs : EventArgs
{
    public PlcDataChangedEventArgs(IReadOnlyDictionary<string, object?> snapshot)
    {
        Snapshot = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(snapshot));
    }

    public IReadOnlyDictionary<string, object?> Snapshot { get; }
}
