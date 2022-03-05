﻿namespace SBRW.Launcher.App.UI_Forms
{
    partial class Parent_Screen
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
            this.Panel_Form_Screens = new System.Windows.Forms.Panel();
            this.PictureBox_Screen_Splash = new System.Windows.Forms.PictureBox();
            this.GroupBox_Launcherlog = new System.Windows.Forms.GroupBox();
            this.TextBox_Live_Log = new System.Windows.Forms.TextBox();
            this.Panel_Splash_Screen = new System.Windows.Forms.Panel();
            this.Clock = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox_Screen_Splash)).BeginInit();
            this.GroupBox_Launcherlog.SuspendLayout();
            this.Panel_Splash_Screen.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_Form_Screens
            // 
            this.Panel_Form_Screens.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Form_Screens.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Panel_Form_Screens.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel_Form_Screens.Location = new System.Drawing.Point(0, 0);
            this.Panel_Form_Screens.Name = "Panel_Form_Screens";
            this.Panel_Form_Screens.Size = new System.Drawing.Size(563, 404);
            this.Panel_Form_Screens.TabIndex = 0;
            this.Panel_Form_Screens.Visible = false;
            // 
            // PictureBox_Screen_Splash
            // 
            this.PictureBox_Screen_Splash.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.PictureBox_Screen_Splash.Location = new System.Drawing.Point(25, 29);
            this.PictureBox_Screen_Splash.Name = "PictureBox_Screen_Splash";
            this.PictureBox_Screen_Splash.Size = new System.Drawing.Size(512, 181);
            this.PictureBox_Screen_Splash.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PictureBox_Screen_Splash.TabIndex = 0;
            this.PictureBox_Screen_Splash.TabStop = false;
            // 
            // GroupBox_Launcherlog
            // 
            this.GroupBox_Launcherlog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.GroupBox_Launcherlog.Controls.Add(this.TextBox_Live_Log);
            this.GroupBox_Launcherlog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GroupBox_Launcherlog.Location = new System.Drawing.Point(25, 216);
            this.GroupBox_Launcherlog.Name = "GroupBox_Launcherlog";
            this.GroupBox_Launcherlog.Size = new System.Drawing.Size(512, 157);
            this.GroupBox_Launcherlog.TabIndex = 28;
            this.GroupBox_Launcherlog.TabStop = false;
            this.GroupBox_Launcherlog.Text = "Log:";
            // 
            // TextBox_Live_Log
            // 
            this.TextBox_Live_Log.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox_Live_Log.Location = new System.Drawing.Point(6, 17);
            this.TextBox_Live_Log.Multiline = true;
            this.TextBox_Live_Log.Name = "TextBox_Live_Log";
            this.TextBox_Live_Log.ReadOnly = true;
            this.TextBox_Live_Log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextBox_Live_Log.Size = new System.Drawing.Size(500, 135);
            this.TextBox_Live_Log.TabIndex = 35;
            // 
            // Panel_Splash_Screen
            // 
            this.Panel_Splash_Screen.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Splash_Screen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Panel_Splash_Screen.Controls.Add(this.PictureBox_Screen_Splash);
            this.Panel_Splash_Screen.Controls.Add(this.GroupBox_Launcherlog);
            this.Panel_Splash_Screen.Location = new System.Drawing.Point(0, 0);
            this.Panel_Splash_Screen.Name = "Panel_Splash_Screen";
            this.Panel_Splash_Screen.Size = new System.Drawing.Size(563, 404);
            this.Panel_Splash_Screen.TabIndex = 29;
            // 
            // Clock
            // 
            this.Clock.Enabled = true;
            this.Clock.Interval = 1200;
            // 
            // Parent_Screen
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(563, 404);
            this.Controls.Add(this.Panel_Form_Screens);
            this.Controls.Add(this.Panel_Splash_Screen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Parent_Screen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Parent_Screen";
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox_Screen_Splash)).EndInit();
            this.GroupBox_Launcherlog.ResumeLayout(false);
            this.GroupBox_Launcherlog.PerformLayout();
            this.Panel_Splash_Screen.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Panel_Form_Screens;
        private System.Windows.Forms.PictureBox PictureBox_Screen_Splash;
        private System.Windows.Forms.GroupBox GroupBox_Launcherlog;
        private System.Windows.Forms.TextBox TextBox_Live_Log;
        private System.Windows.Forms.Panel Panel_Splash_Screen;
        private System.Windows.Forms.Timer Clock;
    }
}