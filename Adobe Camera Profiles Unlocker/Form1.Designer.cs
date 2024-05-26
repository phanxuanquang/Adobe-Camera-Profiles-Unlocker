namespace Adobe_Camera_Profiles_Unlocker
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle10 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle12 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle11 = new DataGridViewCellStyle();
            ProfileTable = new DataGridView();
            No = new DataGridViewTextBoxColumn();
            Profile = new DataGridViewTextBoxColumn();
            InputSearchBox = new TextBox();
            ExportBtn = new Button();
            OutputSearchBox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)ProfileTable).BeginInit();
            SuspendLayout();
            // 
            // ProfileTable
            // 
            ProfileTable.AllowUserToAddRows = false;
            ProfileTable.AllowUserToDeleteRows = false;
            ProfileTable.AllowUserToResizeRows = false;
            ProfileTable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ProfileTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ProfileTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            ProfileTable.BackgroundColor = SystemColors.ButtonFace;
            dataGridViewCellStyle10.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.BackColor = SystemColors.ControlLight;
            dataGridViewCellStyle10.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridViewCellStyle10.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle10.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = DataGridViewTriState.True;
            ProfileTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
            ProfileTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ProfileTable.Columns.AddRange(new DataGridViewColumn[] { No, Profile });
            ProfileTable.Cursor = Cursors.Hand;
            dataGridViewCellStyle12.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle12.BackColor = SystemColors.Window;
            dataGridViewCellStyle12.Font = new Font("Segoe UI", 10F);
            dataGridViewCellStyle12.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle12.SelectionBackColor = SystemColors.GradientActiveCaption;
            dataGridViewCellStyle12.SelectionForeColor = SystemColors.ControlText;
            dataGridViewCellStyle12.WrapMode = DataGridViewTriState.False;
            ProfileTable.DefaultCellStyle = dataGridViewCellStyle12;
            ProfileTable.Location = new Point(12, 129);
            ProfileTable.Name = "ProfileTable";
            ProfileTable.ReadOnly = true;
            ProfileTable.RowHeadersWidth = 62;
            ProfileTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ProfileTable.Size = new Size(918, 651);
            ProfileTable.TabIndex = 0;
            ProfileTable.CellDoubleClick += ProfileTable_CellDoubleClick;
            // 
            // No
            // 
            No.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle11.Alignment = DataGridViewContentAlignment.MiddleCenter;
            No.DefaultCellStyle = dataGridViewCellStyle11;
            No.HeaderText = "No.";
            No.MinimumWidth = 8;
            No.Name = "No";
            No.ReadOnly = true;
            No.Width = 80;
            // 
            // Profile
            // 
            Profile.HeaderText = "Camera Profile";
            Profile.MinimumWidth = 8;
            Profile.Name = "Profile";
            Profile.ReadOnly = true;
            // 
            // InputSearchBox
            // 
            InputSearchBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            InputSearchBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            InputSearchBox.Font = new Font("Segoe UI", 12F);
            InputSearchBox.Location = new Point(12, 70);
            InputSearchBox.Name = "InputSearchBox";
            InputSearchBox.Size = new Size(374, 39);
            InputSearchBox.TabIndex = 2;
            InputSearchBox.KeyDown += InputSearchBox_KeyDown;
            // 
            // ExportBtn
            // 
            ExportBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ExportBtn.BackColor = SystemColors.ActiveCaption;
            ExportBtn.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ExportBtn.Location = new Point(785, 37);
            ExportBtn.Name = "ExportBtn";
            ExportBtn.Size = new Size(145, 72);
            ExportBtn.TabIndex = 3;
            ExportBtn.Text = "Export";
            ExportBtn.UseVisualStyleBackColor = false;
            ExportBtn.Click += ExportBtn_Click;
            // 
            // OutputSearchBox
            // 
            OutputSearchBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OutputSearchBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            OutputSearchBox.Font = new Font("Segoe UI", 12F);
            OutputSearchBox.Location = new Point(392, 70);
            OutputSearchBox.Name = "OutputSearchBox";
            OutputSearchBox.Size = new Size(386, 39);
            OutputSearchBox.TabIndex = 4;
            OutputSearchBox.KeyDown += OutputSearchBox_KeyDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 42);
            label1.Name = "label1";
            label1.Size = new Size(175, 25);
            label1.TabIndex = 5;
            label1.Text = "Input Camera Model";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(392, 37);
            label2.Name = "label2";
            label2.Size = new Size(187, 25);
            label2.TabIndex = 6;
            label2.Text = "Output camera model";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(942, 792);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(OutputSearchBox);
            Controls.Add(ExportBtn);
            Controls.Add(InputSearchBox);
            Controls.Add(ProfileTable);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Adobe Camera Profiles Unlocker";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)ProfileTable).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView ProfileTable;
        private TextBox InputSearchBox;
        private Button ExportBtn;
        private DataGridViewTextBoxColumn No;
        private DataGridViewTextBoxColumn Profile;
        private TextBox OutputSearchBox;
        private Label label1;
        private Label label2;
    }
}
