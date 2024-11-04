using Engineer;
using Engineer.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.System.Profile;

namespace Adobe_Camera_Profiles_Unlocker_Neo
{
    public sealed partial class MainWindow : Window
    {
        private readonly string CameraProfilesDir_ACR;

        private List<string> ModelDirs;
        private List<string> SelectedProfileDirs;

        private ObservableCollection<string> DataSource;
        private ObservableCollection<CameraProfile> CameraProfiles;

        public MainWindow()
        {
            this.InitializeComponent();
            this.AppWindow.TitleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.HideIconAndSystemMenu;
            this.AppWindow.Title = "Adobe Camera Profiles Unlocker Neo - by Phan Xuan Quang";

            CameraProfilesDir_ACR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Adobe\CameraRaw\CameraProfiles");

            ModelDirs = [];
            SelectedProfileDirs = [];
            DataSource = [];
            ProfileTable.ItemsSource = CameraProfiles = [];

            this.Activated += MainWindow_Activated;
            this.InputSearchBox.TextChanged += InputSearchBox_TextChanged;
            this.OutputSearchBox.TextChanged += OutputSearchBox_TextChanged;
            this.InputSearchBox.SuggestionChosen += InputSearchBox_SuggestionChosen;
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            try
            {
                await CheckUpdate("Neo 2.1.1");

                if (!IsWindowsVersionSupported())
                {
                    await ShowDialog("The current version of your Windows is not supported by this application.", false);

                    Application.Current.Exit();
                }

                if (!WindowsHelper.IsUserAdmin())
                {
                    await ShowDialog("The application must be ran with the administrator right.", false);

                    Application.Current.Exit();
                }

                if (!DirectoryHelper.IsDirectoryExist(DirectoryConstant.CameraRawRootDir))
                {
                    var result = await ShowDialog("Camera Raw has not been installed on your device.", false);

                    if (result == ContentDialogResult.Primary)
                    {
                        await Launcher.LaunchUriAsync(new Uri("https://www.adobe.com/go/acr_installer_win"));
                    }

                    Application.Current.Exit();
                }

                if (!DirectoryHelper.IsDirectoryExist(DirectoryConstant.VariantProfileFoldersDir) && !DirectoryHelper.IsDirectoryExist(DirectoryConstant.VariantProfileFoldersDirAlt))
                {
                    await ShowDialog("Cannot load camera profiles from Adobe. Try reinstalling Camera Raw, and Adobe Lightroom or Adobe Photoshop with default installation settings.", false);

                    Application.Current.Exit();
                }

                DirectoryHelper.CreateDirectoryIfNotExist(CameraProfilesDir_ACR);
                DirectoryHelper.CreateDirectoryIfNotExist(DirectoryConstant.CameraRaw3rdPartyProfilesDir);

                var modelDirsTask = DirectoryHelper.GetFolders(DirectoryConstant.VariantProfileFoldersDir);
                var modelDirsAltTask = DirectoryHelper.GetFolders(DirectoryConstant.VariantProfileFoldersDirAlt, false);

                await Task.WhenAll(modelDirsTask, modelDirsAltTask);

                var modelDirs = modelDirsTask.Result;
                var modelDirsAlt = modelDirsAltTask.Result;

                if (modelDirs.Count == 0)
                {
                    ContentDialog noDcpDialog = new()
                    {
                        XamlRoot = RootGrid.XamlRoot,
                        Title = "Error",
                        Content = "Cannot load default profiles of Camera Raw. Try reinstalling Camera Raw, and Adobe Lightroom or Adobe Photoshop.",
                        PrimaryButtonText = "Download Camera Raw",
                        CloseButtonText = "Skip",
                        DefaultButton = ContentDialogButton.Primary
                    };

                    if (await noDcpDialog.ShowAsync() == ContentDialogResult.Primary)
                    {
                        await Launcher.LaunchUriAsync(new Uri("https://www.adobe.com/go/acr_installer_win"));
                        Application.Current.Exit();
                    }
                }
                else
                {
                    ModelDirs.AddRange(modelDirs);
                }

                if (modelDirsAlt.Count == 0)
                {
                    ContentDialog noXmlDialog = new()
                    {
                        XamlRoot = RootGrid.XamlRoot,
                        Title = "Error",
                        Content = "Cannot load full camera profiles of Adobe, Sigma, Nikon, Panasonic, and Fujifilm. Try reinstalling Camera Raw, and Adobe Lightroom or Adobe Photoshop with default installation settings.",
                        PrimaryButtonText = "Download Camera Raw",
                        CloseButtonText = "Skip",
                        DefaultButton = ContentDialogButton.Primary
                    };

                    if (await noXmlDialog.ShowAsync() == ContentDialogResult.Primary)
                    {
                        await Launcher.LaunchUriAsync(new Uri("https://www.adobe.com/go/acr_installer_win"));
                        Application.Current.Exit();
                    }
                }
                else
                {
                    ModelDirs.AddRange(modelDirsAlt);
                }

                if (ModelDirs.Count == 0)
                {
                    await ShowDialog("Cannot load camera profiles from Adobe. Try reinstalling Camera Raw, and Adobe Lightroom or Adobe Photoshop.", false);

                    Application.Current.Exit();
                }

                var models = ModelDirs
                    .AsParallel()
                    .Select(Path.GetFileName)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList();

                foreach (var model in models)
                {
                    DataSource.Add(model);
                }

                InputSearchBox.ItemsSource = OutputSearchBox.ItemsSource = DataSource;
            }
            catch (Exception ex)
            {
                await ShowDialog($"Error while starting the application. {ex.Message}", false);
                Application.Current.Exit();
            }
        }

        #region SearchBox Events
        private async void InputSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            CameraProfiles.Clear();
            SelectedProfileDirs.Clear();

            var cameraDirs = ModelDirs
                .Where(dir => dir.EndsWith(args.SelectedItem.ToString()))
                .Distinct()
                .ToList();

            if (cameraDirs.Count == 0)
            {
                return;
            }

            foreach (var cameraDir in cameraDirs)
            {
                if (!DirectoryHelper.IsDirectoryExist(cameraDir))
                {
                    continue;
                }

                SelectedProfileDirs.AddRange(await DirectoryHelper.GetProfiles(cameraDir));
            }

            try
            {
                var profileNames = SelectedProfileDirs
                    .Select(Path.GetFileName)
                    .ToArray();

                for (int i = 0; i < profileNames.Length; i++)
                {
                    CameraProfiles.Add(new CameraProfile
                    {
                        No = i + 1,
                        ProfileName = profileNames[i]
                        .Replace(".dcp", string.Empty)
                        .Replace(".xmp", string.Empty)
                    });
                }

            }
            catch (Exception ex)
            {
                await ShowDialog($"Cannot find the profiles. {ex.Message}", false);
            }

        }
        private void InputSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = GetFilteredCameras(sender.Text);
            }
        }
        private void OutputSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = GetFilteredCameras(sender.Text);
            }
        }
        #endregion

        #region Button Events
        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputSearchBox.Text))
            {
                await ShowDialog("Please input the code name of the camera model you want to take camera profiles.", false);
                return;
            }

            if (string.IsNullOrEmpty(OutputSearchBox.Text))
            {
                await ShowDialog("Please input the code name of your camera model.", false);
                return;
            }

            if (!ModelDirs.Exists(dir => dir.Contains(OutputSearchBox.Text.Trim()) || dir.Contains(InputSearchBox.Text.Trim())))
            {
                await ShowDialog("Please input the correct code name of the camera models", false);
                return;
            }

            if (InputSearchBox.Text.Equals(OutputSearchBox.Text))
            {
                await ShowDialog("Please restart the Lightroom and the Photoshop applications to apply changes.");
                return;
            }

            if (ProfileTable.SelectedItems.Count == 0)
            {
                await ShowDialog("Please select at least one camera profile to export.", false);
                return;
            }

            var selectedProfiles = ProfileTable.SelectedItems
                  .AsParallel()
                  .Cast<CameraProfile>()
                  .Select(p => p.ProfileName)
                  .ToList();

            foreach (var profile in selectedProfiles)
            {
                var newProfile = SelectedProfileDirs.FirstOrDefault(dir => dir.Contains(profile));

                if (!string.IsNullOrEmpty(newProfile))
                {
                    if (newProfile.EndsWith(".dcp"))
                    {
                        var xmlPath = DcpHelper.AsXML(newProfile);
                        ContentModifier.ModifyXml(xmlPath, InputSearchBox.Text, OutputSearchBox.Text);
                        DcpHelper.AsDCP(xmlPath, Path.Combine(CameraProfilesDir_ACR, $"{profile} (for {OutputSearchBox.Text})"));
                        DcpHelper.AsDCP(xmlPath, Path.Combine(DirectoryConstant.CameraRaw3rdPartyProfilesDir, $"{profile} (for {OutputSearchBox.Text})"));

                        File.Delete(xmlPath);
                    }
                    else
                    {
                        ContentModifier.ModifyXmp(newProfile, OutputSearchBox.Text);
                    }
                }
            }

            ProfileTable.SelectedItems.Clear();

            ContentDialog successDialog = new()
            {
                XamlRoot = RootGrid.XamlRoot,
                Title = "Success",
                PrimaryButtonText = VietcombankBtn.Content.ToString(),
                SecondaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            };

            TextBlock contentText = new()
            {
                Text = "Please restart the Adobe applications to apply changes!",
                TextWrapping = TextWrapping.Wrap
            };

            SymbolIcon successIcon = new()
            {
                Symbol = Symbol.Accept,
                Foreground = new SolidColorBrush(Colors.Green),
                Width = 24,
                Height = 24
            };

            StackPanel contentPanel = new()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8
            };

            contentPanel.Children.Add(successIcon);
            contentPanel.Children.Add(contentText);

            successDialog.Content = contentPanel;

            if (await successDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await Launcher.LaunchUriAsync(new Uri(VietcombankBtn.NavigateUri.AbsoluteUri));
            }
        }
        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var files = Directory.GetFiles(CameraProfilesDir_ACR)
                            .Concat(Directory.GetFiles(DirectoryConstant.CameraRaw3rdPartyProfilesDir))
                            .Where(f => f.EndsWith(".dcp"))
                            .ToArray();

                if (files.Length == 0)
                {
                    await ShowDialog("You have not installed any profiles yet.", true);

                    return;
                }

                ContentDialog confirmationDialog = new()
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "Confirmation",
                    Content = "All newly created camera profiles will be deleted permanently.\n · Yes: Delete all camera profiles\n · No: Select camera profiles and delete them manually",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close
                };

                var result = await confirmationDialog.ShowAsync();

                switch (result)
                {
                    case ContentDialogResult.Primary:
                        Parallel.ForEach(files, file =>
                        {
                            File.Delete(file);
                        });

                        var deletedFileNames = files
                            .AsParallel()
                            .Select(f => f
                                .Replace(CameraProfilesDir_ACR, string.Empty)
                                .Replace(DirectoryConstant.CameraRaw3rdPartyProfilesDir, string.Empty)
                                .Replace("\\", string.Empty)
                                .Replace(".dcp", string.Empty)
                                .Trim())
                            .Distinct()
                            .OrderBy(f => f)
                            .ToArray();

                        await ShowDialog($"Deleted camera profiles:\n · {string.Join("\n · ", deletedFileNames)}");

                        break;

                    case ContentDialogResult.Secondary:
                        WindowsHelper.OpenFolderInExplorer(CameraProfilesDir_ACR);
                        WindowsHelper.OpenFolderInExplorer(DirectoryConstant.CameraRaw3rdPartyProfilesDir);
                        break;
                }
            }
            catch (Exception ex)
            {
                await ShowDialog(ex.Message, false);
            }
        }
        #endregion

        #region Helpers
        public static bool IsWindowsVersionSupported()
        {
            var minOsVersion = new Version(10, 0, 17763, 0);

            var version = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong versionAsLong = ulong.Parse(version);
            ulong major = (versionAsLong & 0xFFFF000000000000L) >> 48;
            ulong minor = (versionAsLong & 0x0000FFFF00000000L) >> 32;
            ulong build = (versionAsLong & 0x00000000FFFF0000L) >> 16;
            ulong revision = (versionAsLong & 0x000000000000FFFFL);

            Version currentOsVersion = new((int)major, (int)minor, (int)build, (int)revision);
            return currentOsVersion.CompareTo(minOsVersion) >= 0;
        }
        private List<string> GetFilteredCameras(string keyword)
        {
            if (string.IsNullOrEmpty(keyword) || string.IsNullOrWhiteSpace(keyword))
            {
                return
                [
                    "No camera found"
                ];
            }

            keyword = keyword.Replace(" ", string.Empty).Trim();

            try
            {
                var results = ModelDirs
                    .AsParallel()
                    .Where(dir => dir.Replace(" ", string.Empty).Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .Select(Path.GetFileName)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList();

                if (results.Count == 0)
                {
                    return
                    [
                        "No camera found"
                    ];
                }

                return results;
            }
            catch
            {
                return
                [
                    "Error while finding the camera"
                ];
            }
        }
        private async Task CheckUpdate(string currentVersion)
        {
            var latestRelease = await AppUpdater.GetGithubLatestReleaseInfo();

            if (latestRelease != null && !currentVersion.Equals(latestRelease.Name, StringComparison.OrdinalIgnoreCase))
            {
                var sb = new StringBuilder();
                sb.AppendLine($"The latest version '{latestRelease.Name}' was released on {latestRelease.CreatedAt.ToString("MMMM dd, yyyy")}.");
                sb.AppendLine();
                sb.AppendLine("Please check the description of the latest version as below:");
                sb.AppendLine(latestRelease.Body);
                sb.AppendLine();
                sb.AppendLine($"It is recommended to download and use the latest version for smoothest experience.");

                var noti = new ContentDialog
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "New Version!",
                    Content = sb.ToString().Trim(),
                    PrimaryButtonText = "Download Now",
                    CloseButtonText = "Skip",
                    DefaultButton = ContentDialogButton.Primary
                };

                if (await noti.ShowAsync() == ContentDialogResult.Primary)
                {
                    await Launcher.LaunchUriAsync(new Uri(latestRelease.Assets[0].BrowserDownloadUrl));
                    Application.Current.Exit();
                }
            }
        }
        private async Task<ContentDialogResult> ShowDialog(string message, bool isSuccess = true, string buttonText = "OK")
        {
            var dialog = new ContentDialog
            {
                XamlRoot = RootGrid.XamlRoot,
                Title = isSuccess ? "Success" : "Error",
                Content = message,
                PrimaryButtonText = buttonText,
                DefaultButton = ContentDialogButton.Primary
            };

            return await dialog.ShowAsync();
        }
        #endregion
    }
}