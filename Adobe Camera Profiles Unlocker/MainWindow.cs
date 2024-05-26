using System.Diagnostics;

namespace Adobe_Camera_Profiles_Unlocker
{
    public partial class MainWindow : Form
    {
        private string[] ModelDirs;
        private List<string> SelectedProfileDirs = new List<string>();
        private AutoCompleteStringCollection DataSource = new AutoCompleteStringCollection();
        private readonly string ModelsDir = @"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera";
        private readonly string[] Brands = { "Canon", "Leica", "Nikon", "Olympus", "Panasonic", "Pentax", "Ricoh", "Sony" };
        public MainWindow()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.icon;

            InputSearchBox.AutoCompleteCustomSource = OutputSearchBox.AutoCompleteCustomSource = DataSource;
            InputSearchBox.AutoCompleteSource = OutputSearchBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ModelDirs = GetChilds(ModelsDir);
            var models = ModelDirs.Select(Path.GetFileName).ToArray();
            DataSource.AddRange(models);
        }

        public string[] GetChilds(string folderPath)
        {
            return Directory.GetDirectories(folderPath);
        }

        static List<string> GetFiles(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.dcp").ToList();
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

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            if (InputSearchBox.Text == OutputSearchBox.Text)
            {
                MessageBox.Show("Complete. Please restart your Lightroom and your Photoshop if they are currently opened.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
            var windowsUsername = Environment.UserName;
            var brandName = string.Empty;

            foreach (DataGridViewRow row in ProfileTable.Rows)
            {
                if (row.Selected)
                {
                    selectedRows.Add(row);
                }
            }

            if (!selectedRows.Any())
            {
                MessageBox.Show("Please select at least one Camera Profile to export.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            foreach (string brand in Brands)
            {
                if (InputSearchBox.Text.Contains(brand))
                {
                    brandName = brand;
                    break;
                }
            }

            foreach (DataGridViewRow row in selectedRows)
            {
                var selectedProfileName = row.Cells[1].Value.ToString();
                var dcpPath = SelectedProfileDirs.FirstOrDefault(i => i.Contains(selectedProfileName));
                var inputXml = AsXML(dcpPath);
                UpdateXMLContent(inputXml, brandName);
                AsDCP(inputXml, @$"C:\Users\{windowsUsername}\AppData\Roaming\Adobe\CameraRaw\CameraProfiles\{brandName} - {selectedProfileName}");
                File.Delete(inputXml);
            }

            MessageBox.Show("Complete. Please restart your Lightroom and your Photoshop if they are currently opened.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            var executer = Process.Start(exeInfor);
            executer.WaitForExit();
        }

        private void UpdateXMLContent(string filePath, string brandName)
        {
            var lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("<ProfileName>"))
                {
                    lines[i] = lines[i].Replace("Camera", $"{brandName}:");
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

        private void OutputSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            var searchResult = InputSearchBox.Text.Trim();
            var cameraDir = ModelDirs.FirstOrDefault(dir => dir.Contains(searchResult));
        }
    }
}
