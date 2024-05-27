using System.Diagnostics;
using System.Security.Policy;
using System.Security.Principal;

namespace Adobe_Camera_Profiles_Unlocker
{
    public partial class MainWindow : Form
    {
        private readonly string ModelsDir = @"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera";
        private readonly string[] Brands = { "Canon", "Leica", "Nikon", "Olympus", "Panasonic", "Pentax", "Ricoh", "Sony", "Sigma" };
        private string[] ModelDirs;
        private string CameraProfilesDir;
        private List<string> SelectedProfileDirs = new List<string>();
        private AutoCompleteStringCollection DataSource = new AutoCompleteStringCollection();
        public MainWindow()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.icon;

            InputSearchBox.AutoCompleteCustomSource = OutputSearchBox.AutoCompleteCustomSource = DataSource;
            InputSearchBox.AutoCompleteSource = OutputSearchBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        #region Events
        private void Form_Load(object sender, EventArgs e)
        {
            CameraProfilesDir = @$"C:\Users\{Environment.UserName}\AppData\Roaming\Adobe\CameraRaw\CameraProfiles";

            if (!Directory.Exists(ModelsDir) || !Directory.Exists(CameraProfilesDir))
            {
                MessageBox.Show("Cannot find Lightroom or Camera Raw on your device.\nPlease install the latest version of Adobe Camera Raw or Adobe Lightroom.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            if (!IsUserAdministrator())
            {
                MessageBox.Show("The application must be ran with the administrator right.\nPlease try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            ModelDirs = GetChilds(ModelsDir);
            var models = ModelDirs.Select(Path.GetFileName).ToArray();
            DataSource.AddRange(models);
        }
        private void ProfileTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow selectedRow = ProfileTable.Rows[e.RowIndex];
                if (selectedRow.Selected)
                {
                    selectedRow.Selected = false;
                }
            }
        }
        private void InputSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            var searchResult = InputSearchBox.Text.Trim();
            var cameraDir = ModelDirs.FirstOrDefault(dir => dir.Contains(searchResult));

            if (!Directory.Exists(cameraDir))
            {
                return;
            }

            SelectedProfileDirs = GetFiles(cameraDir);
            var profileNames = GetFiles(cameraDir).Select(Path.GetFileName).ToArray();
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

            var searchResult = InputSearchBox.Text.Trim();
            var cameraDir = ModelDirs.FirstOrDefault(dir => dir.Contains(searchResult));
        }
        private void ExportBtn_Click(object sender, EventArgs e)
        {
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

            var brandName = Brands.FirstOrDefault(brand => InputSearchBox.Text.Contains(brand)) ?? string.Empty;

            foreach (DataGridViewRow row in selectedRows)
            {
                var selectedProfileName = row.Cells[1].Value.ToString();

                var dcpPath = SelectedProfileDirs.FirstOrDefault(dir => dir.Contains(selectedProfileName));

                if (string.IsNullOrEmpty(dcpPath))
                {
                    continue;
                }

                var inputXml = AsXML(dcpPath);
                UpdateXMLContent(inputXml, brandName);
                AsDCP(inputXml, Path.Combine(CameraProfilesDir, $"{brandName} - {selectedProfileName}"));
                File.Delete(inputXml);
            }

            MessageBox.Show("Please restart the Lightroom and the Photoshop applications if they are currently opened to apply changes.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        public bool IsUserAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private string[] GetChilds(string folderPath)
        {
            return Directory.GetDirectories(folderPath);
        }
        private List<string> GetFiles(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.dcp").ToList();
        }

        private string AsXML(string dcpPath)
        {
            var xmlPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\{Path.GetFileName(dcpPath).Replace(".dcp", ".xml")}";

            var exeInfor = new ProcessStartInfo();
            exeInfor.FileName = @"dcpTool.exe";
            exeInfor.CreateNoWindow = true;
            exeInfor.Arguments = $"-d \"{dcpPath}\" \"{xmlPath}\"";

            var executer = Process.Start(exeInfor);
            executer.WaitForExit();

            return xmlPath;
        }
        private void AsDCP(string inputPath, string outputPath)
        {
            var exeInfor = new ProcessStartInfo();
            exeInfor.FileName = @"dcpTool.exe";
            exeInfor.Arguments = $"-c \"{inputPath}\" \"{outputPath}.dcp\"";
            exeInfor.CreateNoWindow = true;

            var executer = Process.Start(exeInfor);
            executer.WaitForExit();
        }
        private void UpdateXMLContent(string filePath, string brand)
        {
            var lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("<ProfileName>"))
                {
                    lines[i] = lines[i].Replace("Camera", $"{brand}:");
                }

                if (lines[i].Contains("Copyright"))
                {
                    lines[i] = lines[i].Replace(lines[i], "<Copyright>Copyright 2024 Phan Xuan Quang / Github: @phanxuanquang</Copyright>");
                }

                if (lines[i].Contains("<ProfileCalibrationSignature>"))
                {
                    lines[i] = lines[i].Replace("com.adobe", "Phan Xuan Quang");
                }

                if (lines[i].Contains("<UniqueCameraModelRestriction>"))
                {
                    lines[i] = lines[i].Replace(lines[i], $"<UniqueCameraModelRestriction>{OutputSearchBox.Text}</UniqueCameraModelRestriction>");
                }
            }

            File.WriteAllLines(filePath, lines);
        }

        private void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
                CreateNoWindow = true
            });
        }
        private void Flickr_Click(object sender, EventArgs e)
        {
            OpenUrl("https://www.flickr.com/photos/pxq2002");
        }

        private void Instagram_Click(object sender, EventArgs e)
        {
            OpenUrl("https://www.instagram.com/pxquang.2002");
        }

        private void Github_Click(object sender, EventArgs e)
        {
            OpenUrl("https://github.com/phanxuanquang");
        }

        private void TikTok_Click(object sender, EventArgs e)
        {
            OpenUrl("https://www.tiktok.com/@tuyenoaminhnhan");
        }
    }
}
