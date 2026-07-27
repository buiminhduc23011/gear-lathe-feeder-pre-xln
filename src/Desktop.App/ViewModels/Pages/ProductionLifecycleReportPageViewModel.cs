using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.App.Configuration;
using Desktop.App.Configuration.Plc;
using Desktop.App.Models.Reports;
using Desktop.App.Services.Abstractions;

namespace Desktop.App.ViewModels.Pages;

public partial class ProductionLifecycleReportPageViewModel : ObservableObject, IDisposable
{
    private const int DefaultPageSize = 10;

    private readonly IProductionLifecycleReportApiService _reportApiService;
    private readonly IPlcService _plcService;
    private readonly PeriodicTimer _pollingTimer;
    private readonly CancellationTokenSource _pollingCts = new();
    private readonly List<ProductionLifecycleDeclarationDto> _allItems = [];

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private int selectedShelfIndex;

    [ObservableProperty]
    private int selectedStatusIndex;

    [ObservableProperty]
    private int totalDeclarations;

    [ObservableProperty]
    private int inProgressDeclarations;

    [ObservableProperty]
    private int completedDeclarations;

    [ObservableProperty]
    private int failedDeclarations;

    [ObservableProperty]
    private DateTimeOffset? generatedAtUtc;

    [ObservableProperty]
    private int pageIndex = 1;

    [ObservableProperty]
    private int maxPageCount = 1;

    public ObservableCollection<ProductionLifecycleDeclarationDto> Items { get; } = [];

    public string PageStatusText => $"Trang {PageIndex}/{MaxPageCount}";

    public bool CanGoPreviousPage => PageIndex > 1;

    public bool CanGoNextPage => PageIndex < MaxPageCount;

    public ProductionLifecycleReportPageViewModel(IProductionLifecycleReportApiService reportApiService, IPlcService plcService)
        : this(reportApiService, plcService, TimeSpan.FromSeconds(10), startPolling: true)
    {
    }

    internal ProductionLifecycleReportPageViewModel(
        IProductionLifecycleReportApiService reportApiService,
        IPlcService plcService,
        TimeSpan pollingInterval,
        bool startPolling)
    {
        _reportApiService = reportApiService;
        _plcService = plcService;
        _pollingTimer = new PeriodicTimer(pollingInterval);

        if (startPolling)
        {
            _ = StartPollingAsync(_pollingCts.Token);
        }
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            var report = await _reportApiService.GetReportAsync(
                AppSettings.Current.MachineCode,
                GetSelectedStatus(),
                GetSelectedShelf(),
                CancellationToken.None);
            if (report is null)
            {
                return;
            }

            TotalDeclarations = report.TotalDeclarations;
            InProgressDeclarations = report.InProgressDeclarations;
            CompletedDeclarations = report.CompletedDeclarations;
            FailedDeclarations = report.FailedDeclarations;
            GeneratedAtUtc = report.GeneratedAtUtc;

            _allItems.Clear();
            foreach (var item in report.Items.OrderByDescending(x => x.CreatedAtUtc))
            {
                item.IsLoadingParameters = GetEffectiveLoadingStatus(item);
                _allItems.Add(item);
            }

            MaxPageCount = Math.Max(1, (int)Math.Ceiling((double)_allItems.Count / DefaultPageSize));
            if (PageIndex > MaxPageCount)
            {
                PageIndex = MaxPageCount;
            }

            RefreshPageItems();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnPageIndexChanged(int value)
    {
        OnPropertyChanged(nameof(PageStatusText));
        OnPropertyChanged(nameof(CanGoPreviousPage));
        OnPropertyChanged(nameof(CanGoNextPage));
    }

    partial void OnMaxPageCountChanged(int value)
    {
        OnPropertyChanged(nameof(PageStatusText));
        OnPropertyChanged(nameof(CanGoPreviousPage));
        OnPropertyChanged(nameof(CanGoNextPage));
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        PageIndex = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void GoToFirstPage()
    {
        if (PageIndex == 1) return;
        PageIndex = 1;
        RefreshPageItems();
    }

    [RelayCommand]
    private void GoToPreviousPage()
    {
        if (!CanGoPreviousPage) return;
        PageIndex--;
        RefreshPageItems();
    }

    [RelayCommand]
    private void GoToNextPage()
    {
        if (!CanGoNextPage) return;
        PageIndex++;
        RefreshPageItems();
    }

    [RelayCommand]
    private void GoToLastPage()
    {
        if (PageIndex == MaxPageCount) return;
        PageIndex = MaxPageCount;
        RefreshPageItems();
    }

    private string? GetSelectedStatus() => SelectedStatusIndex switch
    {
        1 => "Created",
        2 => "AgvTaken",
        3 => "Loaded",
        4 => "InProduction",
        5 => "Completed",
        6 => "Cleared",
        7 => "Cancelled",
        _ => null
    };

    private int? GetSelectedShelf() => SelectedShelfIndex switch
    {
        1 => 1,
        2 => 2,
        _ => null
    };

    private void RefreshPageItems()
    {
        Items.Clear();
        foreach (var item in _allItems.Skip((PageIndex - 1) * DefaultPageSize).Take(DefaultPageSize))
        {
            Items.Add(item);
        }
    }

    private async Task StartPollingAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _pollingTimer.WaitForNextTickAsync(cancellationToken))
            {
                await LoadDataAsync();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private bool GetEffectiveLoadingStatus(ProductionLifecycleDeclarationDto item)
    {
        var shelfIndex = item.ShelfIndex;
        if (shelfIndex == 1)
        {
            return _plcService.GetValue(PlcTagCatalog.DataAutos.OrderLine1IsLoading.Name, item.IsLoadingParameters);
        }

        if (shelfIndex == 2)
        {
            return _plcService.GetValue(PlcTagCatalog.DataAutos.OrderLine2IsLoading.Name, item.IsLoadingParameters);
        }

        return item.IsLoadingParameters;
    }

    public void Dispose()
    {
        _pollingCts.Cancel();
        _pollingCts.Dispose();
        _pollingTimer.Dispose();
    }
}
