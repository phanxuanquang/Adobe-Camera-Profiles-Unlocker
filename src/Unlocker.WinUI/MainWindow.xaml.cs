using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Unlocker.Presentation.Shell;
using Unlocker.Presentation.Unlock;
using Unlocker.WinUI.Pages;
using Windows.Graphics;

namespace Unlocker.WinUI;

public sealed partial class MainWindow : Window
{
    private const int WidthDip = 1280, HeightDip = 800, MinWidthDip = 760, MinHeightDip = 560;

    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForWindow(nint hwnd);

    public MainWindow(ShellViewModel viewModel, UnlockViewModel unlock)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.SetIcon("Assets/AppIcon.ico");

        // AppWindow sizes are physical pixels: scale DIPs by monitor DPI.
        var scale = GetDpiForWindow(Win32Interop.GetWindowFromWindowId(AppWindow.Id)) / 96.0;
        AppWindow.Resize(new SizeInt32((int)(WidthDip * scale), (int)(HeightDip * scale)));
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(MinWidthDip * scale);
            presenter.PreferredMinimumHeight = (int)(MinHeightDip * scale);
        }

        // Window is not a FrameworkElement: x:Bind here cannot use resource converters, so badges are set in code.
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ShellViewModel.InstalledCount))
                InstalledBadge.Visibility = ViewModel.InstalledCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        };
        unlock.ViewInstalledRequested += (_, _) => Navigate(typeof(InstalledPage));
        NavFrame.Navigate(typeof(StartupPage));
        Activated += OnFirstActivated;
    }

    public ShellViewModel ViewModel { get; }

    private async void OnFirstActivated(object sender, WindowActivatedEventArgs args)
    {
        Activated -= OnFirstActivated; // The old app re-ran init on every activation.
        await ViewModel.InitializeAsync();
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args) => NavView.IsPaneOpen = !NavView.IsPaneOpen;

    private void Navigate(Type page)
    {
        if (NavFrame.CurrentSourcePageType != page) NavFrame.Navigate(page);
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var page = args.IsSettingsSelected ? typeof(SettingsPage) : (args.SelectedItem as NavigationViewItem)?.Tag switch
        {
            "unlock" => typeof(UnlockPage),
            "installed" => typeof(InstalledPage),
            _ => null,
        };
        if (page is not null) Navigate(page);
    }

    /// <summary>Keeps the nav selection in sync when pages navigate by themselves.</summary>
    private void NavFrame_Navigated(object sender, NavigationEventArgs e)
    {
        NavView.IsPaneVisible = e.SourcePageType != typeof(StartupPage);
        NavView.SelectedItem = e.SourcePageType == typeof(SettingsPage) ? NavView.SettingsItem
            : NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(i => (string)i.Tag == (e.SourcePageType == typeof(InstalledPage) ? "installed" : "unlock"));
    }
}
