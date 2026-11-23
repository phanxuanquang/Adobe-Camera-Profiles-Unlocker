using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unlocker.Core.Models;
using Unlocker.Presentation.Settings;

namespace Unlocker.WinUI.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DcpCard.Description = ViewModel.ProfileFolders[0];
        XmpCard.Description = ViewModel.ProfileFolders[1];
    }

    public SettingsViewModel ViewModel { get; } = App.Services.GetRequiredService<SettingsViewModel>();

    public static string VersionText(Version v) => $"Version {v.ToString(3)}";
    public static string DownloadText(ReleaseInfo? r) => r is null ? "Download" : $"Download {r.Version}";

    private void OpenDcpFolder_Click(object sender, RoutedEventArgs e) => ViewModel.OpenFolderCommand.Execute(ViewModel.ProfileFolders[0]);

    private void OpenXmpFolder_Click(object sender, RoutedEventArgs e) => ViewModel.OpenFolderCommand.Execute(ViewModel.ProfileFolders[1]);
}
