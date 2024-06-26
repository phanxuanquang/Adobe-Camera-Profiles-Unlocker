﻿using System.Drawing;
using System.Windows.Forms;

namespace Adobe_Camera_Profiles_Unlocker_2._0
{
    partial class MainWindows
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.InputSearchBox = new Guna.UI2.WinForms.Guna2TextBox();
            this.OutputSearchBox = new Guna.UI2.WinForms.Guna2TextBox();
            this.ProfileTable = new Guna.UI2.WinForms.Guna2DataGridView();
            this.No = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Profile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ExportBtn = new Guna.UI2.WinForms.Guna2GradientTileButton();
            this.label1 = new System.Windows.Forms.Label();
            this.Flickr = new System.Windows.Forms.PictureBox();
            this.Instagram = new System.Windows.Forms.PictureBox();
            this.TikTok = new System.Windows.Forms.PictureBox();
            this.Github = new System.Windows.Forms.PictureBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ResetBtn = new Guna.UI2.WinForms.Guna2GradientTileButton();
            ((System.ComponentModel.ISupportInitialize)(this.ProfileTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Flickr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Instagram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TikTok)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Github)).BeginInit();
            this.SuspendLayout();
            // 
            // InputSearchBox
            // 
            this.InputSearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InputSearchBox.Animated = true;
            this.InputSearchBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.InputSearchBox.AutoSize = true;
            this.InputSearchBox.BorderRadius = 5;
            this.InputSearchBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.InputSearchBox.DefaultText = "";
            this.InputSearchBox.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.InputSearchBox.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.InputSearchBox.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.InputSearchBox.DisabledState.Parent = this.InputSearchBox;
            this.InputSearchBox.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.InputSearchBox.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.InputSearchBox.FocusedState.Parent = this.InputSearchBox;
            this.InputSearchBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.InputSearchBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.InputSearchBox.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.InputSearchBox.HoverState.Parent = this.InputSearchBox;
            this.InputSearchBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.InputSearchBox.Location = new System.Drawing.Point(14, 52);
            this.InputSearchBox.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.InputSearchBox.Name = "InputSearchBox";
            this.InputSearchBox.PasswordChar = '\0';
            this.InputSearchBox.PlaceholderText = "Take the Camera Profiles from . . .  ";
            this.InputSearchBox.SelectedText = "";
            this.InputSearchBox.ShadowDecoration.Parent = this.InputSearchBox;
            this.InputSearchBox.Size = new System.Drawing.Size(867, 43);
            this.InputSearchBox.TabIndex = 10;
            this.InputSearchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputSearchBox_KeyDown);
            // 
            // OutputSearchBox
            // 
            this.OutputSearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputSearchBox.Animated = true;
            this.OutputSearchBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.OutputSearchBox.AutoSize = true;
            this.OutputSearchBox.BorderRadius = 5;
            this.OutputSearchBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.OutputSearchBox.DefaultText = "";
            this.OutputSearchBox.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.OutputSearchBox.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.OutputSearchBox.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.OutputSearchBox.DisabledState.Parent = this.OutputSearchBox;
            this.OutputSearchBox.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.OutputSearchBox.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.OutputSearchBox.FocusedState.Parent = this.OutputSearchBox;
            this.OutputSearchBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.OutputSearchBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.OutputSearchBox.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.OutputSearchBox.HoverState.Parent = this.OutputSearchBox;
            this.OutputSearchBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.OutputSearchBox.Location = new System.Drawing.Point(891, 52);
            this.OutputSearchBox.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.OutputSearchBox.Name = "OutputSearchBox";
            this.OutputSearchBox.PasswordChar = '\0';
            this.OutputSearchBox.PlaceholderText = "And import them into . . . ";
            this.OutputSearchBox.SelectedText = "";
            this.OutputSearchBox.ShadowDecoration.Parent = this.OutputSearchBox;
            this.OutputSearchBox.Size = new System.Drawing.Size(487, 43);
            this.OutputSearchBox.TabIndex = 2;
            this.OutputSearchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OutputSearchBox_KeyDown);
            // 
            // ProfileTable
            // 
            this.ProfileTable.AllowUserToAddRows = false;
            this.ProfileTable.AllowUserToDeleteRows = false;
            this.ProfileTable.AllowUserToResizeRows = false;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
            this.ProfileTable.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle7;
            this.ProfileTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProfileTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ProfileTable.BackgroundColor = System.Drawing.Color.White;
            this.ProfileTable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ProfileTable.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.ProfileTable.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(88)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            dataGridViewCellStyle8.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ProfileTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle8;
            this.ProfileTable.ColumnHeadersHeight = 4;
            this.ProfileTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.No,
            this.Profile});
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            dataGridViewCellStyle9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(69)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(69)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.ProfileTable.DefaultCellStyle = dataGridViewCellStyle9;
            this.ProfileTable.EnableHeadersVisualStyles = false;
            this.ProfileTable.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.ProfileTable.Location = new System.Drawing.Point(14, 107);
            this.ProfileTable.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ProfileTable.Name = "ProfileTable";
            this.ProfileTable.ReadOnly = true;
            this.ProfileTable.RowHeadersVisible = false;
            this.ProfileTable.RowHeadersWidth = 62;
            this.ProfileTable.RowTemplate.Height = 28;
            this.ProfileTable.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ProfileTable.Size = new System.Drawing.Size(1718, 896);
            this.ProfileTable.TabIndex = 3;
            this.ProfileTable.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            this.ProfileTable.ThemeStyle.AlternatingRowsStyle.BackColor = System.Drawing.Color.White;
            this.ProfileTable.ThemeStyle.AlternatingRowsStyle.Font = null;
            this.ProfileTable.ThemeStyle.AlternatingRowsStyle.ForeColor = System.Drawing.Color.Empty;
            this.ProfileTable.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = System.Drawing.Color.Empty;
            this.ProfileTable.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = System.Drawing.Color.Empty;
            this.ProfileTable.ThemeStyle.BackColor = System.Drawing.Color.White;
            this.ProfileTable.ThemeStyle.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.ProfileTable.ThemeStyle.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(88)))), ((int)(((byte)(255)))));
            this.ProfileTable.ThemeStyle.HeaderStyle.BorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.ProfileTable.ThemeStyle.HeaderStyle.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.ProfileTable.ThemeStyle.HeaderStyle.ForeColor = System.Drawing.Color.White;
            this.ProfileTable.ThemeStyle.HeaderStyle.HeaightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            this.ProfileTable.ThemeStyle.HeaderStyle.Height = 4;
            this.ProfileTable.ThemeStyle.ReadOnly = true;
            this.ProfileTable.ThemeStyle.RowsStyle.BackColor = System.Drawing.Color.White;
            this.ProfileTable.ThemeStyle.RowsStyle.BorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.ProfileTable.ThemeStyle.RowsStyle.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.ProfileTable.ThemeStyle.RowsStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(69)))), ((int)(((byte)(94)))));
            this.ProfileTable.ThemeStyle.RowsStyle.Height = 28;
            this.ProfileTable.ThemeStyle.RowsStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.ProfileTable.ThemeStyle.RowsStyle.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(69)))), ((int)(((byte)(94)))));
            // 
            // No
            // 
            this.No.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.No.HeaderText = "No.";
            this.No.MinimumWidth = 8;
            this.No.Name = "No";
            this.No.ReadOnly = true;
            this.No.Width = 80;
            // 
            // Profile
            // 
            this.Profile.HeaderText = "Camera Profile";
            this.Profile.MinimumWidth = 8;
            this.Profile.Name = "Profile";
            this.Profile.ReadOnly = true;
            // 
            // ExportBtn
            // 
            this.ExportBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportBtn.Animated = true;
            this.ExportBtn.BorderRadius = 6;
            this.ExportBtn.CheckedState.Parent = this.ExportBtn;
            this.ExportBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ExportBtn.CustomImages.Parent = this.ExportBtn;
            this.ExportBtn.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.ExportBtn.ForeColor = System.Drawing.Color.White;
            this.ExportBtn.HoverState.Parent = this.ExportBtn;
            this.ExportBtn.Location = new System.Drawing.Point(1387, 52);
            this.ExportBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ExportBtn.Name = "ExportBtn";
            this.ExportBtn.ShadowDecoration.Parent = this.ExportBtn;
            this.ExportBtn.Size = new System.Drawing.Size(167, 43);
            this.ExportBtn.TabIndex = 4;
            this.ExportBtn.Text = "EXECUTE";
            this.ExportBtn.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            this.ExportBtn.TextTransform = Guna.UI2.WinForms.Enums.TextTransform.UpperCase;
            this.ExportBtn.Click += new System.EventHandler(this.ExportBtn_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label1.Location = new System.Drawing.Point(13, 1014);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 30);
            this.label1.TabIndex = 5;
            this.label1.Text = "Find me on: ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Flickr
            // 
            this.Flickr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Flickr.BackgroundImage = global::Adobe_Camera_Profiles_Unlocker_2._0.Properties.Resources.Flickr;
            this.Flickr.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Flickr.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Flickr.Location = new System.Drawing.Point(140, 1013);
            this.Flickr.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Flickr.Name = "Flickr";
            this.Flickr.Size = new System.Drawing.Size(38, 34);
            this.Flickr.TabIndex = 6;
            this.Flickr.TabStop = false;
            this.toolTip.SetToolTip(this.Flickr, "Flickr");
            this.Flickr.Click += new System.EventHandler(this.Flickr_Click);
            // 
            // Instagram
            // 
            this.Instagram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Instagram.BackgroundImage = global::Adobe_Camera_Profiles_Unlocker_2._0.Properties.Resources.Instagram;
            this.Instagram.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Instagram.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Instagram.Location = new System.Drawing.Point(180, 1013);
            this.Instagram.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Instagram.Name = "Instagram";
            this.Instagram.Size = new System.Drawing.Size(38, 34);
            this.Instagram.TabIndex = 7;
            this.Instagram.TabStop = false;
            this.toolTip.SetToolTip(this.Instagram, "Instagram");
            this.Instagram.Click += new System.EventHandler(this.Instagram_Click);
            // 
            // TikTok
            // 
            this.TikTok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TikTok.BackgroundImage = global::Adobe_Camera_Profiles_Unlocker_2._0.Properties.Resources.TikTok;
            this.TikTok.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.TikTok.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TikTok.Location = new System.Drawing.Point(221, 1013);
            this.TikTok.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TikTok.Name = "TikTok";
            this.TikTok.Size = new System.Drawing.Size(38, 34);
            this.TikTok.TabIndex = 8;
            this.TikTok.TabStop = false;
            this.toolTip.SetToolTip(this.TikTok, "TikTok");
            this.TikTok.Click += new System.EventHandler(this.TikTok_Click);
            // 
            // Github
            // 
            this.Github.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Github.BackgroundImage = global::Adobe_Camera_Profiles_Unlocker_2._0.Properties.Resources.Github;
            this.Github.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Github.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Github.Location = new System.Drawing.Point(261, 1013);
            this.Github.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Github.Name = "Github";
            this.Github.Size = new System.Drawing.Size(38, 34);
            this.Github.TabIndex = 9;
            this.Github.TabStop = false;
            this.toolTip.SetToolTip(this.Github, "Github");
            this.Github.Click += new System.EventHandler(this.Github_Click);
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 200;
            this.toolTip.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label2.Location = new System.Drawing.Point(15, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(187, 28);
            this.label2.TabIndex = 11;
            this.label2.Text = "Input camera model";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label3.Location = new System.Drawing.Point(891, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(180, 28);
            this.label3.TabIndex = 12;
            this.label3.Text = "Your camera model";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ResetBtn
            // 
            this.ResetBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetBtn.Animated = true;
            this.ResetBtn.BorderRadius = 6;
            this.ResetBtn.CheckedState.Parent = this.ResetBtn;
            this.ResetBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ResetBtn.CustomImages.Parent = this.ResetBtn;
            this.ResetBtn.FillColor = System.Drawing.Color.Red;
            this.ResetBtn.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.ResetBtn.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.ResetBtn.ForeColor = System.Drawing.Color.White;
            this.ResetBtn.HoverState.Parent = this.ResetBtn;
            this.ResetBtn.Location = new System.Drawing.Point(1563, 52);
            this.ResetBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ResetBtn.Name = "ResetBtn";
            this.ResetBtn.ShadowDecoration.Parent = this.ResetBtn;
            this.ResetBtn.Size = new System.Drawing.Size(167, 43);
            this.ResetBtn.TabIndex = 13;
            this.ResetBtn.Text = "RESET";
            this.ResetBtn.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            this.ResetBtn.TextTransform = Guna.UI2.WinForms.Enums.TextTransform.UpperCase;
            this.ResetBtn.Click += new System.EventHandler(this.ResetBtn_Click);
            // 
            // MainWindows
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1747, 1058);
            this.Controls.Add(this.ResetBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Github);
            this.Controls.Add(this.TikTok);
            this.Controls.Add(this.Instagram);
            this.Controls.Add(this.Flickr);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ExportBtn);
            this.Controls.Add(this.ProfileTable);
            this.Controls.Add(this.OutputSearchBox);
            this.Controls.Add(this.InputSearchBox);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainWindows";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Adobe Camera Profiles Unlocker";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ProfileTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Flickr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Instagram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TikTok)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Github)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion      
        private Guna.UI2.WinForms.Guna2TextBox InputSearchBox;
        private Guna.UI2.WinForms.Guna2TextBox OutputSearchBox;
        private Guna.UI2.WinForms.Guna2DataGridView ProfileTable;
        private Guna.UI2.WinForms.Guna2GradientTileButton ExportBtn;
        private System.Windows.Forms.DataGridViewTextBoxColumn No;
        private System.Windows.Forms.DataGridViewTextBoxColumn Profile;
        private Label label1;
        private PictureBox Flickr;
        private PictureBox Instagram;
        private PictureBox TikTok;
        private PictureBox Github;
        private ToolTip toolTip;
        private Label label2;
        private Label label3;
        private Guna.UI2.WinForms.Guna2GradientTileButton ResetBtn;
    }
}

