using Engineer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

            ModelDirs = new List<string>();
            SelectedProfileDirs = new List<string>();
            DataSource = new ObservableCollection<string>();
            ProfileTable.ItemsSource = CameraProfiles = new ObservableCollection<CameraProfile>();

            this.Activated += MainWindow_Activated;
            this.InputSearchBox.TextChanged += InputSearchBox_TextChanged;
            this.OutputSearchBox.TextChanged += OutputSearchBox_TextChanged;
            this.InputSearchBox.SuggestionChosen += InputSearchBox_SuggestionChosen;
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            try
            {
                await Task.Delay(300);

                if (!IsSupportedOS())
                {
                    ContentDialog adminDialog = new ContentDialog
                    {
                        XamlRoot = RootGrid.XamlRoot,
                        Title = "Error",
                        Content = "The current version of your Windows is not supported by this application.",
                        PrimaryButtonText = "OK"
                    };
                    await adminDialog.ShowAsync();
                    Application.Current.Exit();
                }

                //if (!GeneralHelper.IsUserAdmin())
                //{
                //    ContentDialog adminDialog = new ContentDialog
                //    {
                //        XamlRoot = RootGrid.XamlRoot,
                //        Title = "Error",
                //        Content = "The application must be ran with the administrator right.",
                //        PrimaryButtonText = "OK",
                //        DefaultButton = ContentDialogButton.Primary
                //    };
                //    await adminDialog.ShowAsync();
                //    Application.Current.Exit();
                //}

                if (!Directory.Exists(CameraRaw.BaseDir))
                {
                    ContentDialog confirmationDialog = new ContentDialog
                    {
                        XamlRoot = RootGrid.XamlRoot,
                        Title = "Confirmation",
                        Content = "The Camera Raw has not been installed on your device.",
                        PrimaryButtonText = "Download Camera Raw",
                        CloseButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Primary
                    };

                    var result = await confirmationDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "https://www.adobe.com/go/acr_installer_win",
                            UseShellExecute = true
                        });
                    }

                    Application.Current.Exit();
                }

                if (!Directory.Exists(CameraRaw.InputModelsDir) && !Directory.Exists(CameraRaw.InputModelsDirAlt))
                {
                    ContentDialog noAdobeDialog = new ContentDialog
                    {
                        XamlRoot = RootGrid.XamlRoot,
                        Title = "Error",
                        Content = "Cannot load camera profiles from Adobe.",
                        PrimaryButtonText = "OK",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await noAdobeDialog.ShowAsync();
                    Application.Current.Exit();
                }

                if (!Directory.Exists(CameraProfilesDir_ACR))
                {
                    Directory.CreateDirectory(CameraProfilesDir_ACR);
                }

                if (!Directory.Exists(CameraRaw.CameraProfilesDir_LR))
                {
                    Directory.CreateDirectory(CameraRaw.CameraProfilesDir_LR);
                }

                var modelDirs = await DirectoryHelper.GetFolders(CameraRaw.InputModelsDir);
                var modelDirsAlt = await DirectoryHelper.GetFolders(CameraRaw.InputModelsDirAlt, false);

                ModelDirs.AddRange(modelDirs);
                ModelDirs.AddRange(modelDirsAlt);

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
                await CheckUpdate("Neo 2.0");
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "Error",
                    Content = $"Cannot start the application. {ex.Message}",
                    PrimaryButtonText = "OK",
                };
                await errorDialog.ShowAsync();
            }
        }

        #region SearchBox Events
        private async void InputSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            CameraProfiles.Clear();
            SelectedProfileDirs.Clear();

            var cameraDirs = ModelDirs
                .Where(dir => dir.EndsWith(args.SelectedItem.ToString()))
                .ToList();

            if (cameraDirs.Count == 0)
            {
                return;
            }

            foreach (var cameraDir in cameraDirs)
            {
                if (!Directory.Exists(cameraDir))
                {
                    continue;
                }

                SelectedProfileDirs.AddRange(DirectoryHelper.GetProfileFiles(cameraDir));
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
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "Error",
                    Content = $"Cannot find the profiles. {ex.Message}",
                    PrimaryButtonText = "OK",
                };
                await errorDialog.ShowAsync();
            }

        }
        private void InputSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                FilterResult(sender);
            }
        }
        private void OutputSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                FilterResult(sender);
            }
        }
        #endregion

        #region Button Events
        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputSearchBox.Text))
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "Error",
                    Content = "Please input the code name of the camera model you want to take camera profiles.",
                    PrimaryButtonText = "OK"
                };
                await errorDialog.ShowAsync();
                return;
            }

            if (string.IsNullOrEmpty(OutputSearchBox.Text))
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "Error",
                    Content = "Please input the code name of your camera model.",
                    PrimaryButtonText = "OK"
                };
                await errorDialog.ShowAsync();
                return;
            }

            if (!ModelDirs.Any(dir => dir.Contains(OutputSearchBox.Text.Trim()) || dir.Contains(InputSearchBox.Text.Trim())))
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "Error",
                    Content = "Please input the correct code name of the camera models",
                    PrimaryButtonText = "OK"
                };
                await errorDialog.ShowAsync();
                return;
            }

            if (InputSearchBox.Text.Equals(OutputSearchBox.Text))
            {
                ContentDialog successDialog = new ContentDialog
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "Success",
                    Content = "Please restart the Lightroom and the Photoshop applications to apply changes.",
                    PrimaryButtonText = "OK"
                };
                await successDialog.ShowAsync();
                return;
            }

            if (ProfileTable.SelectedItems.Count == 0)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "Error",
                    Content = "Please select at least one Camera Profile to export.",
                    PrimaryButtonText = "OK"
                };
                await errorDialog.ShowAsync();
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
                        FileUpdater.ModifyXMLContent(xmlPath, InputSearchBox.Text, OutputSearchBox.Text);
                        DcpHelper.AsDCP(xmlPath, Path.Combine(CameraProfilesDir_ACR, $"{profile} (for {OutputSearchBox.Text})"));
                        DcpHelper.AsDCP(xmlPath, Path.Combine(CameraRaw.CameraProfilesDir_LR, $"{profile} (for {OutputSearchBox.Text})"));

                        File.Delete(xmlPath);
                    }
                    else
                    {
                        FileUpdater.ModifyXMPContent(newProfile, InputSearchBox.Text, OutputSearchBox.Text);
                    }
                }
            }

            ProfileTable.SelectedItems.Clear();

            ContentDialog successDialog2 = new ContentDialog
            {
                XamlRoot = RootGrid.XamlRoot,
                Title = "Success",
                Content = "Please restart the Lightroom and the Photoshop applications to apply changes.",
                PrimaryButtonText = "OK"
            };
            await successDialog2.ShowAsync();
        }
        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var files = Directory.GetFiles(CameraProfilesDir_ACR)
                            .Concat(Directory.GetFiles(CameraRaw.CameraProfilesDir_LR))
                            .Where(f => f.EndsWith(".dcp"))
                            .ToArray();

                if (files.Length == 0)
                {
                    ContentDialog doneDialog = new ContentDialog
                    {
                        XamlRoot = RootGrid.XamlRoot,
                        Title = "Success",
                        Content = "You have not installed any profiles yet.",
                        PrimaryButtonText = "OK",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await doneDialog.ShowAsync();

                    return;
                }

                ContentDialog confirmationDialog = new ContentDialog
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
                                .Replace(CameraRaw.CameraProfilesDir_LR, string.Empty)
                                .Replace("\\", string.Empty)
                                .Replace(".dcp", string.Empty)
                                .Trim())
                            .Distinct()
                            .OrderBy(f => f)
                            .ToArray();

                        ContentDialog successDialog = new ContentDialog
                        {
                            XamlRoot = RootGrid.XamlRoot,
                            Title = "Success",
                            Content = $"Deleted camera profiles:\n · {string.Join("\n · ", deletedFileNames)}",
                            PrimaryButtonText = "OK",
                            DefaultButton = ContentDialogButton.Primary
                        };
                        await successDialog.ShowAsync();
                        break;

                    case ContentDialogResult.Secondary:
                        GeneralHelper.OpenFolderInExplorer(CameraProfilesDir_ACR);
                        GeneralHelper.OpenFolderInExplorer(CameraRaw.CameraProfilesDir_LR);
                        break;
                }
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "Error",
                    Content = ex.Message,
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary
                };
                await errorDialog.ShowAsync();
            }
        }
        #endregion

        #region Helpers
        private void FilterResult(AutoSuggestBox sender)
        {
            var keyword = sender.Text.Replace(" ", string.Empty).ToLower().Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                sender.ItemsSource = new List<string>() { "No camera found" };
                return;
            }

            try
            {
                var matchedDirs = ModelDirs
                    .AsParallel()
                    .Where(dir => dir.Replace(" ", string.Empty).ToLower().Contains(keyword))
                    .Select(Path.GetFileName)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList();

                if (matchedDirs.Count == 0)
                {
                    matchedDirs.Add("No camera found");
                }

                sender.ItemsSource = matchedDirs;
            }
            catch
            {
                sender.ItemsSource = new List<string>() { "Error while finding the camera" };
            }
        }
        public bool IsSupportedOS()
        {
            var minOsVersion = new Version(10, 0, 17763, 0);

            var version = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong versionAsLong = ulong.Parse(version);
            ulong major = (versionAsLong & 0xFFFF000000000000L) >> 48;
            ulong minor = (versionAsLong & 0x0000FFFF00000000L) >> 32;
            ulong build = (versionAsLong & 0x00000000FFFF0000L) >> 16;
            ulong revision = (versionAsLong & 0x000000000000FFFFL);

            Version currentOsVersion = new Version((int)major, (int)minor, (int)build, (int)revision);
            return currentOsVersion.CompareTo(minOsVersion) >= 0;
        }
        private async Task CheckUpdate(string currentVersion)
        {
            var latestRelease = await Updater.GetGithubLatestReleaseInfo();

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
        #endregion
    }
}
