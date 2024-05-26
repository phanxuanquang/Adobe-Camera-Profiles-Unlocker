namespace WinFormsApp1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox textBoxInputPath;
        private System.Windows.Forms.TextBox textBoxOutputPath;
        private System.Windows.Forms.Button buttonBrowseInput;
        private System.Windows.Forms.Button buttonBrowseOutput;
        private System.Windows.Forms.Button buttonConvert;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            textBoxInputPath = new TextBox();
            textBoxOutputPath = new TextBox();
            buttonBrowseInput = new Button();
            buttonBrowseOutput = new Button();
            buttonConvert = new Button();
            SuspendLayout();
            // 
            // textBoxInputPath
            // 
            textBoxInputPath.Location = new Point(20, 23);
            textBoxInputPath.Margin = new Padding(5, 6, 5, 6);
            textBoxInputPath.Name = "textBoxInputPath";
            textBoxInputPath.Size = new Size(431, 31);
            textBoxInputPath.TabIndex = 0;
            // 
            // textBoxOutputPath
            // 
            textBoxOutputPath.Location = new Point(20, 73);
            textBoxOutputPath.Margin = new Padding(5, 6, 5, 6);
            textBoxOutputPath.Name = "textBoxOutputPath";
            textBoxOutputPath.Size = new Size(431, 31);
            textBoxOutputPath.TabIndex = 1;
            // 
            // buttonBrowseInput
            // 
            buttonBrowseInput.Location = new Point(463, 19);
            buttonBrowseInput.Margin = new Padding(5, 6, 5, 6);
            buttonBrowseInput.Name = "buttonBrowseInput";
            buttonBrowseInput.Size = new Size(125, 44);
            buttonBrowseInput.TabIndex = 2;
            buttonBrowseInput.Text = "Browse...";
            buttonBrowseInput.UseVisualStyleBackColor = true;
            buttonBrowseInput.Click += buttonBrowseInput_Click;
            // 
            // buttonBrowseOutput
            // 
            buttonBrowseOutput.Location = new Point(463, 69);
            buttonBrowseOutput.Margin = new Padding(5, 6, 5, 6);
            buttonBrowseOutput.Name = "buttonBrowseOutput";
            buttonBrowseOutput.Size = new Size(125, 44);
            buttonBrowseOutput.TabIndex = 3;
            buttonBrowseOutput.Text = "Browse...";
            buttonBrowseOutput.UseVisualStyleBackColor = true;
            buttonBrowseOutput.Click += buttonBrowseOutput_Click;
            // 
            // buttonConvert
            // 
            buttonConvert.Location = new Point(20, 123);
            buttonConvert.Margin = new Padding(5, 6, 5, 6);
            buttonConvert.Name = "buttonConvert";
            buttonConvert.Size = new Size(125, 44);
            buttonConvert.TabIndex = 4;
            buttonConvert.Text = "Convert";
            buttonConvert.UseVisualStyleBackColor = true;
            buttonConvert.Click += buttonConvert_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(608, 192);
            Controls.Add(buttonConvert);
            Controls.Add(buttonBrowseOutput);
            Controls.Add(buttonBrowseInput);
            Controls.Add(textBoxOutputPath);
            Controls.Add(textBoxInputPath);
            Margin = new Padding(5, 6, 5, 6);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DCP Converter";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}

