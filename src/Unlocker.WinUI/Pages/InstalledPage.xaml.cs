using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unlocker.Core.Models;
using Unlocker.Presentation.Installed;

namespace Unlocker.WinUI.Pages;

public sealed partial class InstalledPage : Page
{
    public InstalledPage() => InitializeComponent();

    public InstalledViewModel ViewModel { get; } = App.Services.GetRequiredService<InstalledViewModel>();

    public static string Origin(string source, bool legacy) => legacy ? "Created by older version" : $"From {source}";
    public static string RemoveName(string name, string target) => $"Remove {name} for {target}";

    private async void RemoveItem_Click(object sender, RoutedEventArgs e)
        => await ViewModel.RemoveCommand.ExecuteAsync((InstalledCameraProfile)((FrameworkElement)sender).Tag);

    private async void RemoveGroup_Click(object sender, RoutedEventArgs e)
        => await ViewModel.RemoveGroupCommand.ExecuteAsync((InstalledGroup)((FrameworkElement)sender).Tag);

    private void StatusBar_Closed(InfoBar sender, InfoBarClosedEventArgs args) => ViewModel.Status = null;

    private void Page_Unloaded(object sender, RoutedEventArgs e) => GroupsRepeater.ItemsSource = null;
}
