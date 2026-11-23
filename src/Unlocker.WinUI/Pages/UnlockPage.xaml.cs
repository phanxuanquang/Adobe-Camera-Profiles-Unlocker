using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;
using Unlocker.Presentation.Ports;
using Unlocker.Presentation.Startup;
using Unlocker.Presentation.Unlock;

namespace Unlocker.WinUI.Pages;

public sealed partial class UnlockPage : Page
{
    private bool _fillingModels;

    public UnlockPage()
    {
        InitializeComponent();
        ViewModel.PropertyChanged += OnViewModelChanged;
        Unloaded += (_, _) => ViewModel.PropertyChanged -= OnViewModelChanged; // The singleton VM outlives the page.
    }

    public UnlockViewModel ViewModel { get; } = App.Services.GetRequiredService<UnlockViewModel>();

    public static string CountText(int selected, int visible) => selected > 0 ? $"{selected}/{visible}" : $"{visible}";
    public static string CountName(int selected, int visible) => selected > 0 ? $"{selected} selected of {visible} profiles" : $"{visible} profiles";
    public static string RemoveChipName(string name) => $"Remove {name} from selection";
    public static string TargetButtonName(string? model) => $"Your camera: {model}. Change";

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        WarningBar.IsOpen = e.Parameter is IReadOnlyList<EnvironmentIssue> { Count: > 0 };
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
        FillModels();
        if (ViewModel.IsFirstRun)
        {
            InlinePicker.Show(ViewModel.Catalog, null);
            InlinePicker.FocusSearch();
        }
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UnlockViewModel.SelectedBrand)) FillModels();
    }

    private void FillModels()
    {
        if (ViewModel.SelectedBrand is not { } brand) return;
        _fillingModels = true;
        var models = brand.Brand.Models;
        ModelBox.ItemsSource = models
            .Select(m => m == brand.Brand.BestModel && models.Count > 1 ? $"{m.Name} (most camera profiles)" : m.Name)
            .ToList();
        ModelBox.SelectedIndex = models.ToList().IndexOf(brand.SourceModel);
        _fillingModels = false;
    }

    private void ModelBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_fillingModels || ViewModel.SelectedBrand is not { } brand || ModelBox.SelectedIndex < 0) return;
        brand.SourceModel = brand.Brand.Models[ModelBox.SelectedIndex];
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

    private void ChipRemove_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleProfileCommand.Execute((CameraProfileItemViewModel)((FrameworkElement)sender).Tag);
        // The removed chip had focus.
        if (InstallButton.IsEnabled) InstallButton.Focus(FocusState.Keyboard);
        else ProfileSearch.Focus(FocusState.Keyboard);
    }

    private void StatusBar_Closed(InfoBar sender, InfoBarClosedEventArgs args) => ViewModel.Status = null;

    private async void GetCameraRaw_Click(object sender, RoutedEventArgs e)
        => await App.Services.GetRequiredService<ILauncher>().OpenUrlAsync(StartupViewModel.CameraRawDownload);

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
