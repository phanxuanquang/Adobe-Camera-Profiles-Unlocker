using Engineer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Adobe_Camera_Profiles_Unlocker_Neo
{
    public sealed partial class MainWindow : Window
    {
        private readonly string ModelsDir;
        private string[] ModelDirs;
        private readonly string CameraProfilesDir_ACR;
        private readonly string CameraProfilesDir_LR;
        private List<string> SelectedProfileDirs;
        private ObservableCollection<string> DataSource;

        public ObservableCollection<CameraProfile> CameraProfiles { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();

            this.AppWindow.TitleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.HideIconAndSystemMenu;
            this.AppWindow.Title = "Adobe Camera Profiles Unlocker Neo - by Phan Xuan Quang";

            CameraProfilesDir_ACR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Adobe\CameraRaw\CameraProfiles");
            CameraProfilesDir_LR = @"C:\ProgramData\Adobe\CameraRaw\CameraProfiles";
            ModelsDir = @"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera";

            SelectedProfileDirs = new List<string>();
            DataSource = new ObservableCollection<string>();
            ProfileTable.ItemsSource = CameraProfiles = new ObservableCollection<CameraProfile>();

            this.Activated += MainWindow_Activated;
            this.InputSearchBox.TextChanged += InputSearchBox_TextChanged;
            this.OutputSearchBox.TextChanged += OutputSearchBox_TextChanged;
            this.InputSearchBox.SuggestionChosen += InputSearchBox_SuggestionChosen;
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

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            await Task.Delay(150);

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

            if (!Directory.Exists(CameraProfilesDir_ACR))
            {
                Directory.CreateDirectory(CameraProfilesDir_ACR);
            }

            if (!Directory.Exists(CameraProfilesDir_LR))
            {
                Directory.CreateDirectory(CameraProfilesDir_LR);
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

            ModelDirs = DirectoryHelper.GetChilds(ModelsDir);
            var models = ModelDirs.Select(Path.GetFileName).ToArray();
            foreach (var model in models)
            {
                DataSource.Add(model);
            }

            InputSearchBox.ItemsSource = OutputSearchBox.ItemsSource = DataSource;
        }

        private async void InputSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var cameraDir = ModelDirs.FirstOrDefault(dir => dir.Contains(args.SelectedItem.ToString()));

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
        private async void OutputSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                await FilterResult(sender);
            }
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

            if (ProfileTable.SelectedItems.Count == 0)
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

            var selectedProfiles = ProfileTable.SelectedItems
                .AsParallel()
                .Cast<CameraProfile>()
                .Select(p => p.ProfileName)
                .ToList();

            foreach(var profile in selectedProfiles)
            {
                var dcpPath = SelectedProfileDirs.FirstOrDefault(dir => dir.Contains(profile));

                if (!string.IsNullOrEmpty(dcpPath))
                {
                    var xmlPath = DcpHelper.AsXML(dcpPath);
                    DcpHelper.UpdateXMLContent(xmlPath, InputSearchBox.Text, OutputSearchBox.Text);
                    DcpHelper.AsDCP(xmlPath, Path.Combine(CameraProfilesDir_ACR, profile));
                    DcpHelper.AsDCP(xmlPath, Path.Combine(CameraProfilesDir_LR, profile));

                    File.Delete(xmlPath);
                }
            }

            ProfileTable.SelectedItems.Clear();

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
            try
            {
                var files = Directory.GetFiles(CameraProfilesDir_ACR)
                            .Concat(Directory.GetFiles(CameraProfilesDir_LR))
                            .Where(f => f.EndsWith(".dcp"))
                            .ToArray();

                if (files.Length == 0)
                {
                    ContentDialog doneDialog = new ContentDialog
                    {
                        XamlRoot = this.Content.XamlRoot,
                        Title = "Success",
                        Content = "You have not installed any profiles yet.",
                        PrimaryButtonText = "OK"
                    };
                    await doneDialog.ShowAsync();

                    return;
                }

                ContentDialog confirmationDialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "Confirmation",
                    Content = "All newly created camera profiles will be deleted permanently.\n · Yes: Delete all camera profiles\n · No: Select camera profiles and delete them manually",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No",
                    CloseButtonText = "Cancel",
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
                                .Replace(CameraProfilesDir_LR, string.Empty)
                                .Replace("\\", string.Empty)
                                .Replace(".dcp", string.Empty)
                                .Trim())
                            .Distinct()
                            .OrderBy(f => f)
                            .ToArray();

                        ContentDialog successDialog = new ContentDialog
                        {
                            XamlRoot = this.Content.XamlRoot,
                            Title = "Success",
                            Content = $"Deleted camera profiles:\n · {string.Join("\n · ", deletedFileNames)}",
                            PrimaryButtonText = "OK"
                        };
                        await successDialog.ShowAsync();
                        break;

                    case ContentDialogResult.Secondary:
                        GeneralHelper.OpenFolderInExplorer(CameraProfilesDir_ACR);
                        GeneralHelper.OpenFolderInExplorer(CameraProfilesDir_LR);
                        break;
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
    }

    public class CameraProfile
    {
        public int No { get; set; }
        public string ProfileName { get; set; }
    }
}
