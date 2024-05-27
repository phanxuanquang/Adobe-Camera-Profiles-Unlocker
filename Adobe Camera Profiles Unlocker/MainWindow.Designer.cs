namespace Adobe_Camera_Profiles_Unlocker
{
    partial class MainWindow
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            ProfileTable = new DataGridView();
            No = new DataGridViewTextBoxColumn();
            Profile = new DataGridViewTextBoxColumn();
            InputSearchBox = new TextBox();
            ExportBtn = new Button();
            OutputSearchBox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            Flickr = new PictureBox();
            Instagram = new PictureBox();
            Github = new PictureBox();
            TikTok = new PictureBox();
            label3 = new Label();
            toolTip1 = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)ProfileTable).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Flickr).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Instagram).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Github).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TikTok).BeginInit();
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
            ProfileTable.BackgroundColor = Color.White;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = SystemColors.ControlLight;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridViewCellStyle4.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            ProfileTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            ProfileTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ProfileTable.Columns.AddRange(new DataGridViewColumn[] { No, Profile });
            ProfileTable.Cursor = Cursors.Hand;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = SystemColors.Window;
            dataGridViewCellStyle6.Font = new Font("Segoe UI", 10F);
            dataGridViewCellStyle6.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.GradientActiveCaption;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.ControlText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.False;
            ProfileTable.DefaultCellStyle = dataGridViewCellStyle6;
            ProfileTable.Location = new Point(12, 129);
            ProfileTable.Name = "ProfileTable";
            ProfileTable.ReadOnly = true;
            ProfileTable.RowHeadersWidth = 62;
            ProfileTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ProfileTable.Size = new Size(918, 615);
            ProfileTable.TabIndex = 0;
            toolTip1.SetToolTip(ProfileTable, "Select the profiles you need. Double-click to unselect the profiles.");
            ProfileTable.CellDoubleClick += ProfileTable_CellDoubleClick;
            // 
            // No
            // 
            No.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleCenter;
            No.DefaultCellStyle = dataGridViewCellStyle5;
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
            toolTip1.SetToolTip(ExportBtn, "Export selected profiles to your Adobe apps");
            ExportBtn.UseVisualStyleBackColor = false;
            ExportBtn.Click += ExportBtn_Click;
            // 
            // OutputSearchBox
            // 
            OutputSearchBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            OutputSearchBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            OutputSearchBox.Font = new Font("Segoe UI", 12F);
            OutputSearchBox.Location = new Point(392, 70);
            OutputSearchBox.Name = "OutputSearchBox";
            OutputSearchBox.Size = new Size(386, 39);
            OutputSearchBox.TabIndex = 4;
            toolTip1.SetToolTip(OutputSearchBox, "The code name of your camera");
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
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(392, 37);
            label2.Name = "label2";
            label2.Size = new Size(187, 25);
            label2.TabIndex = 6;
            label2.Text = "Output camera model";
            // 
            // Flickr
            // 
            Flickr.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Flickr.BackgroundImage = Properties.Resources.Flickr;
            Flickr.BackgroundImageLayout = ImageLayout.Zoom;
            Flickr.Cursor = Cursors.Hand;
            Flickr.Image = Properties.Resources.Flickr;
            Flickr.InitialImage = Properties.Resources.Flickr;
            Flickr.Location = new Point(176, 750);
            Flickr.Name = "Flickr";
            Flickr.Size = new Size(45, 45);
            Flickr.TabIndex = 8;
            Flickr.TabStop = false;
            toolTip1.SetToolTip(Flickr, "Flickr");
            Flickr.Click += Flickr_Click;
            // 
            // Instagram
            // 
            Instagram.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Instagram.BackgroundImage = Properties.Resources.Instagram;
            Instagram.BackgroundImageLayout = ImageLayout.Zoom;
            Instagram.Cursor = Cursors.Hand;
            Instagram.Image = Properties.Resources.Flickr;
            Instagram.InitialImage = Properties.Resources.Flickr;
            Instagram.Location = new Point(223, 750);
            Instagram.Name = "Instagram";
            Instagram.Size = new Size(45, 45);
            Instagram.TabIndex = 9;
            Instagram.TabStop = false;
            toolTip1.SetToolTip(Instagram, "Instagram");
            Instagram.Click += Instagram_Click;
            // 
            // Github
            // 
            Github.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Github.BackgroundImage = Properties.Resources.TikTok;
            Github.BackgroundImageLayout = ImageLayout.Zoom;
            Github.Cursor = Cursors.Hand;
            Github.Image = Properties.Resources.Flickr;
            Github.InitialImage = Properties.Resources.Flickr;
            Github.Location = new Point(270, 750);
            Github.Name = "Github";
            Github.Size = new Size(45, 45);
            Github.TabIndex = 10;
            Github.TabStop = false;
            toolTip1.SetToolTip(Github, "TikTok");
            Github.Click += Github_Click;
            // 
            // TikTok
            // 
            TikTok.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            TikTok.BackgroundImage = Properties.Resources.Github;
            TikTok.BackgroundImageLayout = ImageLayout.Zoom;
            TikTok.Cursor = Cursors.Hand;
            TikTok.Image = Properties.Resources.Flickr;
            TikTok.InitialImage = Properties.Resources.Flickr;
            TikTok.Location = new Point(317, 750);
            TikTok.Name = "TikTok";
            TikTok.Size = new Size(45, 45);
            TikTok.TabIndex = 11;
            TikTok.TabStop = false;
            toolTip1.SetToolTip(TikTok, "Github");
            TikTok.Click += TikTok_Click;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14F);
            label3.Location = new Point(13, 752);
            label3.Name = "label3";
            label3.Size = new Size(171, 38);
            label3.TabIndex = 12;
            label3.Text = "Find me on: ";
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(942, 806);
            Controls.Add(TikTok);
            Controls.Add(Github);
            Controls.Add(Instagram);
            Controls.Add(Flickr);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(OutputSearchBox);
            Controls.Add(ExportBtn);
            Controls.Add(InputSearchBox);
            Controls.Add(ProfileTable);
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Adobe Camera Profiles Unlocker";
            Load += Form_Load;
            ((System.ComponentModel.ISupportInitialize)ProfileTable).EndInit();
            ((System.ComponentModel.ISupportInitialize)Flickr).EndInit();
            ((System.ComponentModel.ISupportInitialize)Instagram).EndInit();
            ((System.ComponentModel.ISupportInitialize)Github).EndInit();
            ((System.ComponentModel.ISupportInitialize)TikTok).EndInit();
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
        private PictureBox Flickr;
        private PictureBox Instagram;
        private PictureBox Github;
        private PictureBox TikTok;
        private Label label3;
        private ToolTip toolTip1;
    }
}
