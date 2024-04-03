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
            BtnFolderSearch = new Button();
            CboPath = new ComboBox();
            BtnMetadataChange = new Button();
            PrgModificationApply = new ProgressBar();
            LblSelectMetadata = new Label();
            CboMetadataList = new ComboBox();
            LstMetadataValues = new ListBox();
            MnuOptions.SuspendLayout();
            SuspendLayout();
            // 
            // LstFiles
            // 
            LstFiles.FormattingEnabled = true;
            LstFiles.ItemHeight = 15;
            LstFiles.Items.AddRange(new object[] { "Allo", "Comment", "Il", "Va" });
            LstFiles.Location = new Point(392, 71);
            LstFiles.Name = "LstFiles";
            LstFiles.SelectionMode = SelectionMode.MultiSimple;
            LstFiles.Size = new Size(396, 259);
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
            // BtnFolderSearch
            // 
            BtnFolderSearch.Location = new Point(760, 41);
            BtnFolderSearch.Name = "BtnFolderSearch";
            BtnFolderSearch.Size = new Size(28, 25);
            BtnFolderSearch.TabIndex = 3;
            BtnFolderSearch.Text = "...";
            BtnFolderSearch.UseVisualStyleBackColor = true;
            // 
            // CboPath
            // 
            CboPath.AutoCompleteMode = AutoCompleteMode.Suggest;
            CboPath.AutoCompleteSource = AutoCompleteSource.FileSystem;
            CboPath.FormattingEnabled = true;
            CboPath.Location = new Point(392, 42);
            CboPath.Name = "CboPath";
            CboPath.Size = new Size(359, 23);
            CboPath.TabIndex = 4;
            CboPath.DropDown += CboPath_DropDown;
            CboPath.SelectedValueChanged += CboPath_ValueChanged;
            // 
            // BtnMetadataChange
            // 
            BtnMetadataChange.Font = new Font("Segoe UI", 14F);
            BtnMetadataChange.Location = new Point(392, 336);
            BtnMetadataChange.Name = "BtnMetadataChange";
            BtnMetadataChange.Size = new Size(396, 40);
            BtnMetadataChange.TabIndex = 5;
            BtnMetadataChange.Text = "Apply Metadata Change";
            BtnMetadataChange.UseVisualStyleBackColor = true;
            // 
            // PrgModificationApply
            // 
            PrgModificationApply.Location = new Point(12, 414);
            PrgModificationApply.Name = "PrgModificationApply";
            PrgModificationApply.Size = new Size(776, 24);
            PrgModificationApply.TabIndex = 6;
            // 
            // LblSelectMetadata
            // 
            LblSelectMetadata.Font = new Font("Segoe UI", 10F);
            LblSelectMetadata.Location = new Point(17, 46);
            LblSelectMetadata.Name = "LblSelectMetadata";
            LblSelectMetadata.Size = new Size(191, 23);
            LblSelectMetadata.TabIndex = 7;
            LblSelectMetadata.Text = "Select a Metadata to Modify:";
            // 
            // CboMetadataList
            // 
            CboMetadataList.FormattingEnabled = true;
            CboMetadataList.Location = new Point(17, 82);
            CboMetadataList.Name = "CboMetadataList";
            CboMetadataList.Size = new Size(309, 23);
            CboMetadataList.TabIndex = 8;
            // 
            // LstMetadataValues
            // 
            LstMetadataValues.FormattingEnabled = true;
            LstMetadataValues.ItemHeight = 15;
            LstMetadataValues.Location = new Point(40, 129);
            LstMetadataValues.Name = "LstMetadataValues";
            LstMetadataValues.SelectionMode = SelectionMode.None;
            LstMetadataValues.Size = new Size(265, 244);
            LstMetadataValues.TabIndex = 9;
            // 
            // FrmFileManipulator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(LstMetadataValues);
            Controls.Add(CboMetadataList);
            Controls.Add(LblSelectMetadata);
            Controls.Add(PrgModificationApply);
            Controls.Add(BtnMetadataChange);
            Controls.Add(CboPath);
            Controls.Add(BtnFolderSearch);
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
        private Button BtnFolderSearch;
        private ComboBox CboPath;
        private Button BtnMetadataChange;
        private ProgressBar PrgModificationApply;
        private Label LblSelectMetadata;
        private ComboBox CboMetadataList;
        private ListBox LstMetadataValues;
    }
}
