using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonBrowseInput_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "DCP files (*.dcp)|*.dcp|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxInputPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void buttonBrowseOutput_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxOutputPath.Text = saveFileDialog.FileName;
                }
            }
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            var inputPath = textBoxInputPath.Text;
            var outputPath = textBoxOutputPath.Text;

            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
            {
                MessageBox.Show("Please select both input and output paths.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string decompileCommand = $"./dcpTool/dcpTool -d \"{inputPath}\" \"{outputPath}\"";
            ExecuteCommand(decompileCommand);
            MessageBox.Show("Conversion completed!");
        }

        private void ExecuteCommand(string command)
        {
            Process process = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "powershell.exe";
            processStartInfo.Arguments = command;
            processStartInfo.UseShellExecute = true;
            process.StartInfo = processStartInfo;
            process.Start();
        }
    }
}
