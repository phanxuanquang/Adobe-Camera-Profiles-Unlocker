using Engineer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Adobe_Camera_Profiles_Unlocker_Neo
{
    public sealed partial class MainWindow : Window
    {
        private readonly string ModelsDir;
        private string[] ModelDirs;
        private string CameraProfilesDir;
        private List<string> SelectedProfileDirs;
        private ObservableCollection<string> DataSource;

        public ObservableCollection<CameraProfile> CameraProfiles { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();

            this.AppWindow.TitleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.HideIconAndSystemMenu;
            this.AppWindow.Title = "Adobe Camera Profiles Unlocker Neo - by Phan Xuan Quang";

            ModelsDir = @"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera";
            SelectedProfileDirs = new List<string>();
            DataSource = new ObservableCollection<string>();

            CameraProfiles = new ObservableCollection<CameraProfile>();
            ProfileTable.ItemsSource = CameraProfiles;

            this.Activated += MainWindow_Activated;
            this.InputSearchBox.TextChanged += InputSearchBox_TextChanged;
            this.InputSearchBox.SuggestionChosen += InputSearchBox_SuggestionChosen;
            this.OutputSearchBox.TextChanged += OutputSearchBox_TextChanged;
        }

        private async void InputSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var cameraDir = ModelDirs.FirstOrDefault(dir => dir.ToLower().Contains(sender.Text.ToLower().Trim()));

            if (!Directory.Exists(cameraDir))
            {
                return;
            }

            try
            {
                SelectedProfileDirs = DirectoryHelper.GetDcpFiles(cameraDir);
                var profileNames = SelectedProfileDirs.Select(Path.GetFileName).ToArray();
                var cameraPrefix = $"{sender.Text} Camera ";
                CameraProfiles.Clear();

                for (int i = 0; i < profileNames.Length; i++)
                {
                    CameraProfiles.Add(new CameraProfile { No = i + 1, ProfileName = profileNames[i].Replace(".dcp", "").Replace(cameraPrefix, "") });
                }
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Error",
                    Content = ex.Message,
                    PrimaryButtonText = "OK",
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void InputSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                await FilterResult(sender);
            }
        }

        private async Task FilterResult(AutoSuggestBox sender)
        {
            var matchedItems = new List<string>();
            var keyword = sender.Text.ToLower().Trim();

            try
            {
                var matchedDirs = ModelDirs.AsParallel()
                .Where(dir => dir.Replace($"{ModelsDir}\\", string.Empty).ToLower().Contains(keyword))
                .Select(dir => dir.Replace($"{ModelsDir}\\", string.Empty))
                .ToList();

                if (matchedDirs.Count == 0)
                {
                    matchedDirs.Add("No camera found");
                }

                sender.ItemsSource = matchedDirs;
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Error",
                    Content = ex.Message,
                    PrimaryButtonText = "OK",
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void OutputSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                await FilterResult(sender);
            }
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            await Task.Delay(200);

            CameraProfilesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Adobe\CameraRaw\CameraProfiles");

            if (!Directory.Exists(CameraProfilesDir))
            {
                Directory.CreateDirectory(CameraProfilesDir);
            }

            if (!Directory.Exists(ModelsDir))
            {
                ContentDialog noAdobeDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Cannot find Lightroom or Camera Raw on your device.\nPlease install the latest version of Adobe Camera Raw or Adobe Lightroom.",
                    PrimaryButtonText = "OK"
                };
                await noAdobeDialog.ShowAsync();
                Application.Current.Exit();
            }

            if (!GeneralHelper.IsUserAdmin())
            {
                ContentDialog adminDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Error",
                    Content = "The application must be ran with the administrator right.\nPlease try again.",
                    PrimaryButtonText = "OK"
                };
                await adminDialog.ShowAsync();
                Application.Current.Exit();
            }

            ModelDirs = DirectoryHelper.GetChilds(ModelsDir);
            var models = ModelDirs.Select(Path.GetFileName).ToArray();
            foreach (var model in models)
            {
                DataSource.Add(model);
            }
            InputSearchBox.ItemsSource = DataSource;
            OutputSearchBox.ItemsSource = DataSource;
        }

        private async void OutputSearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var searchResult = (sender as ComboBox)?.Text.Trim();

            if (!ModelDirs.Any(dir => dir.Contains(searchResult)))
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Error",
                    Content = "Please input the correct code name of the camera model.",
                    PrimaryButtonText = "OK"
                };
                await errorDialog.ShowAsync();
                OutputSearchBox.Text = string.Empty;
                OutputSearchBox.Focus(FocusState.Programmatic);
                return;
            }
            var cameraDir = ModelDirs.FirstOrDefault(dir => dir.Contains(searchResult));
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputSearchBox.Text.Trim()))
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Error",
                    Content = "Please input the code name of the camera model you want to take camera profiles.",
                    PrimaryButtonText = "OK"
                };
                await errorDialog.ShowAsync();
                return;
            }

            if (string.IsNullOrEmpty(OutputSearchBox.Text.Trim()))
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Error",
                    Content = "Please input the code name of your camera model.",
                    PrimaryButtonText = "OK"
                };
                await errorDialog.ShowAsync();
                return;
            }

            if (!ModelDirs.Any(dir => dir.Contains(OutputSearchBox.Text.Trim())) || !ModelDirs.Any(dir => dir.Contains(InputSearchBox.Text.Trim())))
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Error",
                    Content = "Please input the correct code name of the camera models",
                    PrimaryButtonText = "OK"
                };
                await errorDialog.ShowAsync();
                return;
            }

            if (InputSearchBox.Text == OutputSearchBox.Text)
            {
                ContentDialog successDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Success",
                    Content = "Please restart the Lightroom and the Photoshop applications to apply changes.",
                    PrimaryButtonText = "OK"
                };
                await successDialog.ShowAsync();
                return;
            }

            var selectedItems = ProfileTable.SelectedItems;
            if (selectedItems.Count == 0)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Error",
                    Content = "Please select at least one Camera Profile to export.",
                    PrimaryButtonText = "OK"
                };
                await errorDialog.ShowAsync();
                return;
            }

            foreach (var item in selectedItems)
            {
                try
                {
                    var selectedProfile = item as CameraProfile;
                    var selectedProfileName = selectedProfile.ProfileName;

                    var dcpPath = SelectedProfileDirs.FirstOrDefault(dir => dir.Contains(selectedProfileName));

                    if (string.IsNullOrEmpty(dcpPath))
                    {
                        continue;
                    }

                    var xmlPath = DcpHelper.AsXML(dcpPath);
                    DcpHelper.UpdateXMLContent(xmlPath, InputSearchBox.Text, OutputSearchBox.Text);
                    DcpHelper.AsDCP(xmlPath, Path.Combine(CameraProfilesDir, $"{InputSearchBox.Text} - {selectedProfileName}"));

                    File.Delete(xmlPath);
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        XamlRoot = this.Content.XamlRoot,
                        Title = "Error",
                        Content = $"Failed with the profile '{(item as CameraProfile)?.ProfileName}'.\n{ex.Message}",
                        PrimaryButtonText = "OK"
                    };
                    await errorDialog.ShowAsync();
                }
            }

            ContentDialog successDialog2 = new ContentDialog
            {
                XamlRoot = this.Content.XamlRoot,
                Title = "Success",
                Content = "Please restart the Lightroom and the Photoshop applications to apply changes.",
                PrimaryButtonText = "OK"
            };
            await successDialog2.ShowAsync();
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            CameraProfilesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Adobe\CameraRaw\CameraProfiles");
            if (Directory.Exists(CameraProfilesDir))
            {
                ContentDialog confirmationDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Confirmation",
                    Content = "All newly created camera profiles will be deleted permanently.\n\n- Yes: Delete all camera profiles\n\n- No: Select camera profiles and delete them manually",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No",
                    CloseButtonText = "Cancel",
                };

                var result = await confirmationDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(CameraProfilesDir);
                        if (files.Length > 0)
                        {
                            foreach (string file in files)
                            {
                                File.Delete(file);
                            }
                        }
                        ContentDialog successDialog = new ContentDialog
                        {
                            XamlRoot = this.Content.XamlRoot,
                            Title = "Success",
                            Content = "All camera profiles have been deleted successfully.",
                            PrimaryButtonText = "OK"
                        };
                        await successDialog.ShowAsync();
                    }
                    catch (Exception ex)
                    {
                        ContentDialog errorDialog = new ContentDialog
                        {
                            XamlRoot = this.Content.XamlRoot,
                            Title = "Error",
                            Content = ex.Message,
                            PrimaryButtonText = "OK",
                        };
                        await errorDialog.ShowAsync();
                    }
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        Arguments = CameraProfilesDir,
                        FileName = "explorer.exe"
                    };

                    Process process = new Process
                    {
                        StartInfo = startInfo
                    };

                    process.Start();
                }
                else
                {
                    ContentDialog cancelledDialog = new ContentDialog
                    {
                        XamlRoot = this.Content.XamlRoot,
                        Title = "Cancelled",
                        Content = "File deletion was cancelled.",
                        PrimaryButtonText = "OK"
                    };
                    await cancelledDialog.ShowAsync();
                }
            }
        }
    }

    public class CameraProfile
    {
        public int No { get; set; }
        public string ProfileName { get; set; }
    }
}
