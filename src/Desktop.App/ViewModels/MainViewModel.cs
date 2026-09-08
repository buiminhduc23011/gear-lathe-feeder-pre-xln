using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Models;

namespace Desktop.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private RobotJobInfo _currentJob = new()
    {
        MachineName = "PGR-01",
        CurrentOperation = "Idle",
        LastUpdatedUtc = DateTime.UtcNow,
    };

    public string StatusText => $"Machine: {CurrentJob.MachineName} | Operation: {CurrentJob.CurrentOperation}";

    public string LastUpdatedText => $"Last update (UTC): {CurrentJob.LastUpdatedUtc:yyyy-MM-dd HH:mm:ss}";

    [RelayCommand]
    private void RefreshStatus()
    {
        CurrentJob = new RobotJobInfo
        {
            MachineName = CurrentJob.MachineName,
            CurrentOperation = CurrentJob.CurrentOperation == "Grinding" ? "Idle" : "Grinding",
            LastUpdatedUtc = DateTime.UtcNow,
        };
    }

    partial void OnCurrentJobChanged(RobotJobInfo value)
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(LastUpdatedText));
    }
}
