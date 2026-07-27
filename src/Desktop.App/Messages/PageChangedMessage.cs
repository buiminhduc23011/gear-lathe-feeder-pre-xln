using CommunityToolkit.Mvvm.Messaging.Messages;
using Desktop.App.Models.Ui;

namespace Desktop.App.Messages;

public sealed class PageChangedMessage : ValueChangedMessage<PageType>
{
    public PageChangedMessage(PageType value) : base(value)
    {
    }
}
