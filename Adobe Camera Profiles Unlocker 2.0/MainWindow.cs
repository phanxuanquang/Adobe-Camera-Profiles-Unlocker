using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Adobe_Camera_Profiles_Unlocker_2._0
{
    public partial class MainWindows : Form
    {
        private readonly string ModelsDir;
        private string[] ModelDirs;
        private string CameraProfilesDir;
        private List<string> SelectedProfileDirs;
        private AutoCompleteStringCollection DataSource;
        public MainWindows()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.icon;

            ModelsDir = @"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera";
            SelectedProfileDirs = new List<string>();
            DataSource = new AutoCompleteStringCollection();

            InputSearchBox.AutoCompleteCustomSource = OutputSearchBox.AutoCompleteCustomSource = DataSource;
            InputSearchBox.AutoCompleteSource = OutputSearchBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }
        private void Form_Load(object sender, EventArgs e)
        {
            CameraProfilesDir = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Adobe\CameraRaw\CameraProfiles";

            if (!Directory.Exists(CameraProfilesDir))
            {
                Directory.CreateDirectory(CameraProfilesDir);
            }

            if (!Directory.Exists(ModelsDir))
            {
                MessageBox.Show("Cannot find Lightroom or Camera Raw on your device.\nPlease install the latest version of Adobe Camera Raw or Adobe Lightroom.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            if (!GeneralHelper.IsUserAdmin())
            {
                MessageBox.Show("The application must be ran with the administrator right.\nPlease try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            ModelDirs = DirectoryHelper.GetChilds(ModelsDir);
            var models = ModelDirs.Select(Path.GetFileName).ToArray();
            DataSource.AddRange(models);
        }

        #region Search
        private void InputSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            var searchResult = InputSearchBox.Text.Trim();
            if (!ModelDirs.Any(dir => dir.Contains(searchResult)))
            {
                MessageBox.Show("Please input the correct code name of the camera model.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                InputSearchBox.Text = string.Empty;
                InputSearchBox.Focus();
                return;
            }
            var cameraDir = ModelDirs.FirstOrDefault(dir => dir.Contains(searchResult));

            if (!Directory.Exists(cameraDir))
            {
                return;
            }

            SelectedProfileDirs = DirectoryHelper.GetDcpFiles(cameraDir);
            var profileNames = DirectoryHelper.GetDcpFiles(cameraDir).Select(Path.GetFileName).ToArray();
            var cameraPrefix = $"{searchResult} Camera ";
            ProfileTable.Rows.Clear();

            for (int i = 0; i < profileNames.Length; i++)
            {
                ProfileTable.Rows.Add(i + 1, profileNames[i].Replace(".dcp", null).Replace(cameraPrefix, null));
            }
        }
        private void OutputSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            var searchResult = OutputSearchBox.Text.Trim();

            if (!ModelDirs.Any(dir => dir.Contains(searchResult)))
            {
                MessageBox.Show("Please input the correct code name of the camera model.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                OutputSearchBox.Text = string.Empty;
                OutputSearchBox.Focus();
                return;
            }
            var cameraDir = ModelDirs.FirstOrDefault(dir => dir.Contains(searchResult));
        }
        #endregion

        #region Social Media
        private void Flickr_Click(object sender, EventArgs e)
        {
            GeneralHelper.OpenUrl("https://www.flickr.com/photos/pxq2002");
        }
        private void Instagram_Click(object sender, EventArgs e)
        {
            GeneralHelper.OpenUrl("https://www.instagram.com/pxquang.2002");
        }
        private void Github_Click(object sender, EventArgs e)
        {
            GeneralHelper.OpenUrl("https://github.com/phanxuanquang");
        }
        private void TikTok_Click(object sender, EventArgs e)
        {
            GeneralHelper.OpenUrl("https://www.tiktok.com/@tuyenoaminhnhan");
        }
        #endregion

        #region Main Features
        private void ExportBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(InputSearchBox.Text.Trim()))
            {
                MessageBox.Show("Please input the code name of the camera model you want to take camera profiles.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(OutputSearchBox.Text.Trim()))
            {
                MessageBox.Show("Please input the code name of your camera model.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ModelDirs.Any(dir => dir.Contains(OutputSearchBox.Text.Trim())) || !ModelDirs.Any(dir => dir.Contains(InputSearchBox.Text.Trim())))
            {
                MessageBox.Show("Please input the correct code name of the camera models", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (InputSearchBox.Text == OutputSearchBox.Text)
            {
                MessageBox.Show("Please restart the Lightroom and the Photoshop applications to apply changes.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRows = ProfileTable.SelectedRows;

            if (selectedRows.Count == 0)
            {
                MessageBox.Show("Please select at least one Camera Profile to export.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            foreach (DataGridViewRow row in selectedRows)
            {
                var selectedProfileName = row.Cells[1].Value.ToString();

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

            MessageBox.Show("Please restart the Lightroom and the Photoshop applications to apply changes.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ResetBtn_Click(object sender, EventArgs e)
        {
            CameraProfilesDir = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Adobe\CameraRaw\CameraProfiles";
            if (Directory.Exists(CameraProfilesDir))
            {
                DialogResult result = MessageBox.Show("All newly created camera profiles will be deleted permanently.\n- Yes: Delete all camera profiles\n- No: Select camera profiles and delete them manually", "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
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
                        MessageBox.Show("All camera profiles have been deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (result == DialogResult.No)
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
                    MessageBox.Show("File deletion was cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        #endregion
    }
}
