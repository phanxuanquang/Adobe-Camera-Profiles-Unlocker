using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unlocker.Presentation.Startup;

namespace Unlocker.WinUI.Pages;

public sealed partial class StartupPage : Page
{
    public StartupPage()
    {
        InitializeComponent();
        ViewModel.PropertyChanged += (_, e) =>
        {
            // Non-blocking warnings travel to the Unlock page as the navigation parameter.
            if (e.PropertyName == nameof(StartupViewModel.State) && ViewModel.State == StartupState.Ready)
                Frame.Navigate(typeof(UnlockPage), ViewModel.Warnings);
        };
    }

    public StartupViewModel ViewModel { get; } = App.Services.GetRequiredService<StartupViewModel>();

    private async void Page_Loaded(object sender, RoutedEventArgs e) => await ViewModel.LoadCommand.ExecuteAsync(null);
}
