using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unlocker.Presentation.Ports;

namespace Unlocker.WinUI.Services;

internal sealed class DialogService(MainWindow window) : IDialogService
{
    public async Task<bool> ConfirmAsync(string title, string body, string confirmText)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = window.Content.XamlRoot,
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
}
