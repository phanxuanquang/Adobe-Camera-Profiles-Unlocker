using CommunityToolkit.Labs.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;
using Unlocker.Presentation.Unlock;

namespace Unlocker.WinUI.Pages;

public sealed partial class UnlockPage : Page
{
    public UnlockPage() => InitializeComponent();

    public UnlockViewModel ViewModel { get; } = App.Services.GetRequiredService<UnlockViewModel>();

    public static string CountText(int selected, int visible) => selected > 0 ? $"{selected}/{visible}" : $"{visible}";
    public static string CountName(int selected, int visible) => selected > 0 ? $"{selected} selected of {visible} profiles" : $"{visible} profiles";
    public static string TargetButtonName(string? model) => $"Your camera: {model}. Change";
    public static string InstalledHelp(bool installed) => installed ? "Already installed for your camera." : "";

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        WarningBar.IsOpen = e.Parameter is IReadOnlyList<EnvironmentIssue> { Count: > 0 };
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
        if (ViewModel.IsFirstRun)
        {
            InlinePicker.Show(ViewModel.Catalog, null);
            InlinePicker.FocusSearch();
        }
    }

    private void BrandList_ItemClick(object sender, ItemClickEventArgs e)
    {
        ViewModel.SearchQuery = "";
        ViewModel.SelectedBrand = (BrandItemViewModel)e.ClickedItem;
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearSearchCommand.Execute(null);
        ProfileSearch.Focus(FocusState.Programmatic);
    }

    private void TrayChips_TokenItemRemoving(object? sender, TokenItemRemovingEventArgs e)
    {
        ViewModel.ToggleProfileCommand.Execute((CameraProfileItemViewModel)e.Item);
        // The removed chip had focus.
        if (InstallButton.IsEnabled) InstallButton.Focus(FocusState.Keyboard);
        else ProfileSearch.Focus(FocusState.Keyboard);
    }

    private void StatusBar_Closed(InfoBar sender, InfoBarClosedEventArgs args) => ViewModel.Status = null;

    private async void InlinePicker_Picked(object? sender, string model) => await ViewModel.SetTargetCommand.ExecuteAsync(model);

    private void TargetButton_Click(object sender, RoutedEventArgs e)
    {
        PanePicker.Show(ViewModel.Catalog, ViewModel.TargetCamera);
        CameraPane.IsPaneOpen = true;
    }

    private void CameraPane_PaneOpened(SplitView sender, object args) => PanePicker.FocusSearch();

    private void CloseCameraPane_Click(object sender, RoutedEventArgs e)
    {
        CameraPane.IsPaneOpen = false;
        TargetButton.Focus(FocusState.Programmatic);
    }

    private async void PanePicker_Picked(object? sender, string model)
    {
        CameraPane.IsPaneOpen = false;
        await ViewModel.SetTargetCommand.ExecuteAsync(model);
        TargetButton.Focus(FocusState.Programmatic);
    }
}
