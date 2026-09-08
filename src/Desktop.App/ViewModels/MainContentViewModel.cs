using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Desktop.App.Messages;
using Desktop.App.Models.Ui;
using Desktop.App.Views.Pages;
using System.Windows.Controls;

namespace Desktop.App.ViewModels;

public partial class MainContentViewModel : ObservableObject, IRecipient<PageChangedMessage>
{
    private readonly Func<PageType, UserControl> _createPage;
    private readonly UserControl _autoPage;

    [ObservableProperty]
    private PageType currentPage;

    [ObservableProperty]
    private UserControl currentView;

    public MainContentViewModel()
        : this(CreatePage)
    {
    }

    internal MainContentViewModel(Func<PageType, UserControl> createPage)
    {
        _createPage = createPage;
        _autoPage = _createPage(PageType.Auto);
        currentView = _autoPage;
        CurrentPage = PageType.Auto;
        WeakReferenceMessenger.Default.Register(this);
        SelectPage(CurrentPage);
    }

    public void Receive(PageChangedMessage message)
    {
        CurrentPage = message.Value;
        SelectPage(CurrentPage);
    }

    private void SelectPage(PageType pageType)
    {
        CurrentView = pageType switch
        {
            PageType.Auto => _autoPage,
            PageType.Manual => _createPage(PageType.Manual),
            PageType.Gpio => _createPage(PageType.Gpio),
            PageType.History => _createPage(PageType.History),
            PageType.Settings => _createPage(PageType.Settings),
            PageType.Report => _createPage(PageType.Report),
            PageType.Model => _createPage(PageType.Model),
            _ => _autoPage,
        };
    }

    private static UserControl CreatePage(PageType pageType)
    {
        return pageType switch
        {
            PageType.Auto => new AutoPage(),
            PageType.Manual => new ManualPage(),
            PageType.Gpio => new GpioPage(),
            PageType.History => new HistoryPage(),
            PageType.Settings => new SettingsPage(),
            PageType.Report => new ReportPage(),
            PageType.Model => new ModelPage(),
            _ => new AutoPage(),
        };
    }
}
