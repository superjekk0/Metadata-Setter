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
            cboPath = new ComboBox();
            btnMetadataChange = new Button();
            prgModificationApply = new ProgressBar();
            lblSelectMetadata = new Label();
            cboMetadataList = new ComboBox();
            txtApplyValue = new TextBox();
            LblNewValue = new Label();
            lsvFiles = new ListView();
            ImgIcons = new ImageList(components);
            BtnUpperFolder = new Button();
            numValues = new NumericUpDown();
            lsvMetadataValues = new ListView();
            grpTrackOrder = new GroupBox();
            btnLower = new Button();
            btnHigher = new Button();
            btnDown = new Button();
            btnUp = new Button();
            MnuOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numValues).BeginInit();
            grpTrackOrder.SuspendLayout();
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
            // cboPath
            // 
            cboPath.AutoCompleteMode = AutoCompleteMode.Suggest;
            cboPath.AutoCompleteSource = AutoCompleteSource.FileSystem;
            cboPath.FormattingEnabled = true;
            cboPath.IntegralHeight = false;
            cboPath.Location = new Point(426, 42);
            cboPath.Name = "cboPath";
            cboPath.Size = new Size(325, 23);
            cboPath.TabIndex = 4;
            cboPath.DropDown += CboPath_DropDown;
            // 
            // btnMetadataChange
            // 
            btnMetadataChange.Font = new Font("Segoe UI", 14F);
            btnMetadataChange.Location = new Point(392, 384);
            btnMetadataChange.Name = "btnMetadataChange";
            btnMetadataChange.Size = new Size(396, 40);
            btnMetadataChange.TabIndex = 5;
            btnMetadataChange.Text = "Apply Metadata Change";
            btnMetadataChange.UseVisualStyleBackColor = true;
            btnMetadataChange.Click += BtnMetadataChange_Click;
            // 
            // prgModificationApply
            // 
            prgModificationApply.Location = new Point(12, 446);
            prgModificationApply.Name = "prgModificationApply";
            prgModificationApply.Size = new Size(776, 24);
            prgModificationApply.TabIndex = 6;
            // 
            // lblSelectMetadata
            // 
            lblSelectMetadata.Font = new Font("Segoe UI", 10F);
            lblSelectMetadata.Location = new Point(17, 46);
            lblSelectMetadata.Name = "lblSelectMetadata";
            lblSelectMetadata.Size = new Size(191, 23);
            lblSelectMetadata.TabIndex = 7;
            lblSelectMetadata.Text = "Select a Metadata to Modify:";
            // 
            // cboMetadataList
            // 
            cboMetadataList.AutoCompleteMode = AutoCompleteMode.Suggest;
            cboMetadataList.AutoCompleteSource = AutoCompleteSource.ListItems;
            cboMetadataList.FormattingEnabled = true;
            cboMetadataList.IntegralHeight = false;
            cboMetadataList.Location = new Point(17, 82);
            cboMetadataList.Name = "cboMetadataList";
            cboMetadataList.Size = new Size(309, 23);
            cboMetadataList.TabIndex = 8;
            cboMetadataList.SelectedIndexChanged += CboMetadataList_IndexChanged;
            // 
            // txtApplyValue
            // 
            txtApplyValue.Location = new Point(39, 151);
            txtApplyValue.Name = "txtApplyValue";
            txtApplyValue.Size = new Size(265, 23);
            txtApplyValue.TabIndex = 10;
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
            // lsvFiles
            // 
            lsvFiles.Location = new Point(392, 71);
            lsvFiles.Name = "lsvFiles";
            lsvFiles.Size = new Size(396, 289);
            lsvFiles.SmallImageList = ImgIcons;
            lsvFiles.TabIndex = 12;
            lsvFiles.UseCompatibleStateImageBehavior = false;
            lsvFiles.View = View.Details;
            lsvFiles.SelectedIndexChanged += LsvFiles_SelectedIndexChanged;
            lsvFiles.MouseDoubleClick += LsvFiles_MouseDoubleClick;
            // 
            // ImgIcons
            // 
            ImgIcons.ColorDepth = ColorDepth.Depth32Bit;
            ImgIcons.ImageStream = (ImageListStreamer)resources.GetObject("ImgIcons.ImageStream");
            ImgIcons.TransparentColor = Color.Transparent;
            ImgIcons.Images.SetKeyName(0, "folderIcon.png");
            ImgIcons.Images.SetKeyName(1, "fileIcon.png");
            ImgIcons.Images.SetKeyName(2, "grab2.png");
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
            // numValues
            // 
            numValues.Location = new Point(39, 151);
            numValues.Name = "numValues";
            numValues.Size = new Size(265, 23);
            numValues.TabIndex = 14;
            numValues.Visible = false;
            // 
            // lsvMetadataValues
            // 
            lsvMetadataValues.FullRowSelect = true;
            lsvMetadataValues.Location = new Point(39, 193);
            lsvMetadataValues.MultiSelect = false;
            lsvMetadataValues.Name = "lsvMetadataValues";
            lsvMetadataValues.Size = new Size(265, 231);
            lsvMetadataValues.TabIndex = 15;
            lsvMetadataValues.UseCompatibleStateImageBehavior = false;
            lsvMetadataValues.View = View.Details;
            lsvMetadataValues.MouseDown += LsvMetadataValues_MouseDown;
            lsvMetadataValues.MouseMove += LsvMetadataValues_MouseMove;
            lsvMetadataValues.MouseUp += LsvMetadataValues_MouseUp;
            // 
            // grpTrackOrder
            // 
            grpTrackOrder.BackColor = Color.Transparent;
            grpTrackOrder.Controls.Add(btnLower);
            grpTrackOrder.Controls.Add(btnHigher);
            grpTrackOrder.Controls.Add(btnDown);
            grpTrackOrder.Controls.Add(btnUp);
            grpTrackOrder.Location = new Point(39, 151);
            grpTrackOrder.Name = "grpTrackOrder";
            grpTrackOrder.Size = new Size(265, 23);
            grpTrackOrder.TabIndex = 16;
            grpTrackOrder.TabStop = false;
            grpTrackOrder.Visible = false;
            // 
            // btnLower
            // 
            btnLower.Location = new Point(202, -1);
            btnLower.Name = "btnLower";
            btnLower.Size = new Size(57, 23);
            btnLower.TabIndex = 3;
            btnLower.Text = "Lower";
            btnLower.UseVisualStyleBackColor = true;
            btnLower.Click += BtnLower_Click;
            // 
            // btnHigher
            // 
            btnHigher.Location = new Point(140, 0);
            btnHigher.Name = "btnHigher";
            btnHigher.Size = new Size(56, 23);
            btnHigher.TabIndex = 2;
            btnHigher.Text = "Higher";
            btnHigher.UseVisualStyleBackColor = true;
            btnHigher.Click += BtnHigher_Click;
            // 
            // btnDown
            // 
            btnDown.Location = new Point(60, 0);
            btnDown.Name = "btnDown";
            btnDown.Size = new Size(47, 23);
            btnDown.TabIndex = 1;
            btnDown.Text = "Down";
            btnDown.UseVisualStyleBackColor = true;
            btnDown.Click += BtnDown_Click;
            // 
            // btnUp
            // 
            btnUp.Location = new Point(6, 0);
            btnUp.Name = "btnUp";
            btnUp.Size = new Size(48, 23);
            btnUp.TabIndex = 0;
            btnUp.Text = "Up";
            btnUp.UseVisualStyleBackColor = true;
            btnUp.Click += BtnUp_Click;
            // 
            // FrmFileManipulator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 482);
            Controls.Add(grpTrackOrder);
            Controls.Add(lsvMetadataValues);
            Controls.Add(numValues);
            Controls.Add(BtnUpperFolder);
            Controls.Add(lsvFiles);
            Controls.Add(LblNewValue);
            Controls.Add(txtApplyValue);
            Controls.Add(cboMetadataList);
            Controls.Add(lblSelectMetadata);
            Controls.Add(prgModificationApply);
            Controls.Add(btnMetadataChange);
            Controls.Add(cboPath);
            Controls.Add(BtnFolderSearch);
            Controls.Add(MnuOptions);
            MainMenuStrip = MnuOptions;
            Name = "FrmFileManipulator";
            Text = "Metadata File Manipulator";
            Click += This_Click;
            MnuOptions.ResumeLayout(false);
            MnuOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numValues).EndInit();
            grpTrackOrder.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private MenuStrip MnuOptions;
        private ToolStripMenuItem MnuOptionsFile;
        private ToolStripMenuItem MnuOptionsFileExit;
        private Button BtnFolderSearch;
        private ComboBox cboPath;
        private Button btnMetadataChange;
        private ProgressBar prgModificationApply;
        private Label lblSelectMetadata;
        private ComboBox cboMetadataList;
        private TextBox txtApplyValue;
        private Label LblNewValue;
        private ListView lsvFiles;
        private ImageList ImgIcons;
        private Button BtnUpperFolder;
        private NumericUpDown numValues;
        private ListView lsvMetadataValues;
        private GroupBox grpTrackOrder;
        private Button btnLower;
        private Button btnHigher;
        private Button btnDown;
        private Button btnUp;
    }
}
