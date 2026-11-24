using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unlocker.Presentation.Ports;

namespace Unlocker.WinUI.Services;

// Resolves the window per dialog: MainWindow -> UnlockViewModel -> IDialogService would be a DI cycle at construction.
internal sealed class DialogService(IServiceProvider services) : IDialogService
{
    private XamlRoot Root => services.GetRequiredService<MainWindow>().Content.XamlRoot;

    public async Task<bool> ConfirmAsync(string title, string body, string confirmText)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Root,
            Title = title,
            Content = new ScrollViewer
            {
                MaxHeight = 280,
                Content = new TextBlock { Text = body, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true },
            },
            PrimaryButtonText = confirmText,
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
        };
        // Default is Cancel so Enter never deletes. The verb label carries the danger; no red style, so High Contrast stays correct.
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    public async Task<bool> InformAsync(string title, string body, string primaryText)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Root,
            Title = title,
            Content = new TextBlock { Text = body, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true },
            PrimaryButtonText = primaryText,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }
}
