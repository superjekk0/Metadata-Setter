namespace Metadata_Setter
{
    partial class FrmFileManipulator
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
            LstFiles = new ListBox();
            MnuOptions = new MenuStrip();
            MnuOptionsFile = new ToolStripMenuItem();
            MnuOptionsFileExit = new ToolStripMenuItem();
            TxtFolderPath = new TextBox();
            BtnFolderSearch = new Button();
            MnuOptions.SuspendLayout();
            SuspendLayout();
            // 
            // LstFiles
            // 
            LstFiles.FormattingEnabled = true;
            LstFiles.ItemHeight = 15;
            LstFiles.Items.AddRange(new object[] { "Allo", "Comment", "Il", "Va" });
            LstFiles.Location = new Point(283, 74);
            LstFiles.Name = "LstFiles";
            LstFiles.Size = new Size(396, 199);
            LstFiles.TabIndex = 0;
            // 
            // MnuOptions
            // 
            MnuOptions.Items.AddRange(new ToolStripItem[] { MnuOptionsFile });
            MnuOptions.Location = new Point(0, 0);
            MnuOptions.Name = "MnuOptions";
            MnuOptions.Size = new Size(800, 24);
            MnuOptions.TabIndex = 1;
            MnuOptions.Text = "MnuOptions";
            // 
            // MnuOptionsFile
            // 
            MnuOptionsFile.DropDownItems.AddRange(new ToolStripItem[] { MnuOptionsFileExit });
            MnuOptionsFile.Name = "MnuOptionsFile";
            MnuOptionsFile.Size = new Size(37, 20);
            MnuOptionsFile.Text = "File";
            // 
            // MnuOptionsFileExit
            // 
            MnuOptionsFileExit.Name = "MnuOptionsFileExit";
            MnuOptionsFileExit.Size = new Size(93, 22);
            MnuOptionsFileExit.Text = "Exit";
            MnuOptionsFileExit.Click += MnuOptionsFileExit_Click;
            // 
            // TxtFolderPath
            // 
            TxtFolderPath.Location = new Point(283, 44);
            TxtFolderPath.Name = "TxtFolderPath";
            TxtFolderPath.Size = new Size(359, 23);
            TxtFolderPath.TabIndex = 2;
            // 
            // BtnFolderSearch
            // 
            BtnFolderSearch.Location = new Point(651, 43);
            BtnFolderSearch.Name = "BtnFolderSearch";
            BtnFolderSearch.Size = new Size(28, 25);
            BtnFolderSearch.TabIndex = 3;
            BtnFolderSearch.Text = "button1";
            BtnFolderSearch.UseVisualStyleBackColor = true;
            // 
            // FrmFileManipulator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(BtnFolderSearch);
            Controls.Add(TxtFolderPath);
            Controls.Add(LstFiles);
            Controls.Add(MnuOptions);
            MainMenuStrip = MnuOptions;
            Name = "FrmFileManipulator";
            Text = "Form1";
            MnuOptions.ResumeLayout(false);
            MnuOptions.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox LstFiles;
        private MenuStrip MnuOptions;
        private ToolStripMenuItem MnuOptionsFile;
        private ToolStripMenuItem MnuOptionsFileExit;
        private TextBox TxtFolderPath;
        private Button BtnFolderSearch;
    }
}
