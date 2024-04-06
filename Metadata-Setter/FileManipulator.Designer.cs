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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFileManipulator));
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
            TxtApplyValue = new TextBox();
            LblNewValue = new Label();
            LsvFiles = new ListView();
            ImgIcons = new ImageList(components);
            BtnUpperFolder = new Button();
            MnuOptions.SuspendLayout();
            SuspendLayout();
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
            BtnFolderSearch.Image = (Image)resources.GetObject("BtnFolderSearch.Image");
            BtnFolderSearch.Location = new Point(760, 41);
            BtnFolderSearch.Name = "BtnFolderSearch";
            BtnFolderSearch.Size = new Size(28, 25);
            BtnFolderSearch.TabIndex = 3;
            BtnFolderSearch.UseVisualStyleBackColor = true;
            BtnFolderSearch.Click += BtnFolderSearch_Click;
            // 
            // CboPath
            // 
            CboPath.AutoCompleteMode = AutoCompleteMode.Suggest;
            CboPath.AutoCompleteSource = AutoCompleteSource.FileSystem;
            CboPath.FormattingEnabled = true;
            CboPath.Location = new Point(426, 42);
            CboPath.Name = "CboPath";
            CboPath.Size = new Size(325, 23);
            CboPath.TabIndex = 4;
            CboPath.DropDown += CboPath_DropDown;
            // 
            // BtnMetadataChange
            // 
            BtnMetadataChange.Font = new Font("Segoe UI", 14F);
            BtnMetadataChange.Location = new Point(392, 384);
            BtnMetadataChange.Name = "BtnMetadataChange";
            BtnMetadataChange.Size = new Size(396, 40);
            BtnMetadataChange.TabIndex = 5;
            BtnMetadataChange.Text = "Apply Metadata Change";
            BtnMetadataChange.UseVisualStyleBackColor = true;
            // 
            // PrgModificationApply
            // 
            PrgModificationApply.Location = new Point(12, 446);
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
            CboMetadataList.AutoCompleteMode = AutoCompleteMode.Suggest;
            CboMetadataList.AutoCompleteSource = AutoCompleteSource.ListItems;
            CboMetadataList.FormattingEnabled = true;
            CboMetadataList.Items.AddRange(new object[] { "Album", "Genre", "Title", "Year" });
            CboMetadataList.Location = new Point(17, 82);
            CboMetadataList.Name = "CboMetadataList";
            CboMetadataList.Size = new Size(309, 23);
            CboMetadataList.TabIndex = 8;
            CboMetadataList.SelectedIndexChanged += CboMetadataList_IndexChanged;
            // 
            // LstMetadataValues
            // 
            LstMetadataValues.FormattingEnabled = true;
            LstMetadataValues.ItemHeight = 15;
            LstMetadataValues.Location = new Point(39, 180);
            LstMetadataValues.Name = "LstMetadataValues";
            LstMetadataValues.SelectionMode = SelectionMode.None;
            LstMetadataValues.Size = new Size(265, 244);
            LstMetadataValues.TabIndex = 9;
            // 
            // TxtApplyValue
            // 
            TxtApplyValue.Location = new Point(39, 151);
            TxtApplyValue.Name = "TxtApplyValue";
            TxtApplyValue.Size = new Size(265, 23);
            TxtApplyValue.TabIndex = 10;
            // 
            // LblNewValue
            // 
            LblNewValue.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            LblNewValue.Location = new Point(39, 125);
            LblNewValue.Name = "LblNewValue";
            LblNewValue.Size = new Size(265, 23);
            LblNewValue.TabIndex = 11;
            LblNewValue.Text = "New Value to Apply:";
            // 
            // LsvFiles
            // 
            LsvFiles.Location = new Point(392, 71);
            LsvFiles.Name = "LsvFiles";
            LsvFiles.Size = new Size(396, 289);
            LsvFiles.SmallImageList = ImgIcons;
            LsvFiles.TabIndex = 12;
            LsvFiles.UseCompatibleStateImageBehavior = false;
            LsvFiles.View = View.Details;
            LsvFiles.SelectedIndexChanged += LsvFiles_SelectedIndexChanged;
            LsvFiles.MouseDoubleClick += LsvFiles_MouseDoubleClick;
            // 
            // ImgIcons
            // 
            ImgIcons.ColorDepth = ColorDepth.Depth32Bit;
            ImgIcons.ImageStream = (ImageListStreamer)resources.GetObject("ImgIcons.ImageStream");
            ImgIcons.TransparentColor = Color.Transparent;
            ImgIcons.Images.SetKeyName(0, "folderIcon.png");
            ImgIcons.Images.SetKeyName(1, "fileIcon.png");
            // 
            // BtnUpperFolder
            // 
            BtnUpperFolder.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            BtnUpperFolder.Image = (Image)resources.GetObject("BtnUpperFolder.Image");
            BtnUpperFolder.Location = new Point(392, 41);
            BtnUpperFolder.Name = "BtnUpperFolder";
            BtnUpperFolder.Size = new Size(28, 25);
            BtnUpperFolder.TabIndex = 13;
            BtnUpperFolder.UseVisualStyleBackColor = true;
            BtnUpperFolder.Click += BtnUpperFolder_Click;
            // 
            // FrmFileManipulator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 482);
            Controls.Add(BtnUpperFolder);
            Controls.Add(LsvFiles);
            Controls.Add(LblNewValue);
            Controls.Add(TxtApplyValue);
            Controls.Add(LstMetadataValues);
            Controls.Add(CboMetadataList);
            Controls.Add(LblSelectMetadata);
            Controls.Add(PrgModificationApply);
            Controls.Add(BtnMetadataChange);
            Controls.Add(CboPath);
            Controls.Add(BtnFolderSearch);
            Controls.Add(MnuOptions);
            MainMenuStrip = MnuOptions;
            Name = "FrmFileManipulator";
            Text = "Metadata File Manipulator";
            MnuOptions.ResumeLayout(false);
            MnuOptions.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
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
        private TextBox TxtApplyValue;
        private Label LblNewValue;
        private ListView LsvFiles;
        private ImageList ImgIcons;
        private Button BtnUpperFolder;
    }
}
