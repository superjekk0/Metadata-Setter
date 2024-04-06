// Program written by Charles Mandziuk, (c) 2024
//
// Logiciel écrit par Charles Mandziuk, (c) 2024


namespace Metadata_Setter
{
    public partial class FrmFileManipulator : Form
    {
        private string repository = "";
        public FrmFileManipulator()
        {
            InitializeComponent();
            repository = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + '\\';
            ColumnHeader header = new ColumnHeader
            {
                Text = "",
                Name = "Type",
                Width = LsvFiles.Width
            };

            LsvFiles.Columns.Add(header);
            LsvFiles.HeaderStyle = ColumnHeaderStyle.None;

            CboPath.Items.Add(repository);
            CboPath.Items.AddRange(Directory.GetDirectories(repository).OrderBy(f => f).ToArray());
            CboPath.Text = repository;

            RenderFileTree(CboPath.Text);

            //CboMetadataList.Items.AddRange(new string[]
            //{
            //    "Title",
            //});
        }

        private void MnuOptionsFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        //
        // CboPath
        //
        private void CboPath_DropDown(object sender, EventArgs e)
        {
            CboPath.BeginUpdate();

            foreach (var item in CboPath.Items)
            {
                if (item.ToString() != repository)
                {
                    CboPath.Items.Remove(item);
                }
            }
            CboPath.Items.AddRange(Directory.GetDirectories(repository).Select(d => d += '\\').ToArray());

            CboPath.EndUpdate();
        }

        private void LsvFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem target = LsvFiles.Items[LsvFiles.SelectedIndices[0]];
            if (target.ImageIndex == 0)
            {
                CboPath.Text += target.Text + '\\';
                
                RenderFileTree(CboPath.Text);
            }
        }

        private void BtnFolderSearch_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog()
            {
                InitialDirectory = CboPath.Text
            };

            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                CboPath.Text = dialog.SelectedPath + '\\';
                RenderFileTree(CboPath.Text);
            }
        }

        private void BtnUpperFolder_Click(object sender, EventArgs e)
        {
            DirectoryInfo? parent = Directory.GetParent(repository);
            if (parent != null && parent.Parent != null)
            {
                CboPath.Text = parent.Parent.FullName + (parent.Root.FullName == parent.Parent.FullName ? "" : "\\");
                RenderFileTree(CboPath.Text);
            }
        }

        private void CboMetadataList_IndexChanged(object sender, EventArgs e)
        {
            UpdateTagList(LsvFiles.SelectedIndices);
        }

        private void LsvFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTagList(LsvFiles.SelectedIndices);
        }

        //
        // Various utility functions
        //
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ActiveControl == CboPath && keyData == Keys.Enter)
            {
                RenderFileTree(CboPath.Text + 
                    (CboPath.Text.Length - 1 == CboPath.Text.LastIndexOf('\\') ? "" : "\\"));
                CboPath.Text = repository;
                LsvFiles.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void RenderFileTree(string path)
        {
            var itemsBefore = LsvFiles.Items.OfType<ListViewItem>().ToArray();
            CboPath.BeginUpdate();

            try
            {
                LsvFiles.Items.Clear();
                LsvFiles.Items.AddRange(Directory.GetDirectories(path).Select(d => new ListViewItem(d.Substring(d.LastIndexOf('\\') + 1), 0)).ToArray());
                LsvFiles.Items.AddRange(Directory.GetFiles(path).Select(f => new ListViewItem(f.Substring(f.LastIndexOf('\\') + 1), 1)).ToArray());
                UpdateRepository(path);
            }
            catch (Exception ex)
            {
                LsvFiles.Items.Clear();
                LsvFiles.Items.AddRange(itemsBefore);
                MessageBox.Show(ex.Message, "Directory access", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CboPath.Text = repository;
            }

            CboPath.EndUpdate();

            UpdateTagList(LsvFiles.SelectedIndices);
        }

        private void UpdateRepository(string path)
        {
            repository = path;
        }

        private void UpdateTagList(ListView.SelectedIndexCollection selectedIndices)
        {
            if (CboMetadataList.SelectedIndex == -1)
            {
                return;
            }

            LstMetadataValues.Items.Clear();

            List<TagLib.File> files = new List<TagLib.File>();
            if (selectedIndices.Count != 0)
            {
                files = LsvFiles.Items.OfType<ListViewItem>()
                    .Where(i => i.ImageIndex != 0
                    && TagLib.SupportedMimeType.AllExtensions.Contains(i.Text.Substring(i.Text.LastIndexOf('.') + 1))
                    && selectedIndices.Contains(i.Index))
                    .Select(i => TagLib.File.Create(repository + i.Text))
                    .ToList();
            }
            else
            {
                files = LsvFiles.Items.OfType<ListViewItem>()
                    .Where(i => i.ImageIndex != 0
                    && TagLib.SupportedMimeType.AllExtensions.Contains(i.Text.Substring(i.Text.LastIndexOf('.') + 1)))
                    .Select(i => TagLib.File.Create(repository + i.Text))
                    .ToList();
            }

            LstMetadataValues.Items.AddRange(FileTags(files, CboMetadataList.Text));
        }

        private object[] FileTags(List<TagLib.File> files, string tag)
        {
            IEnumerable<object> result = new List<object>();

            switch (tag)
            {
                case "Album":
                    return files.Select(f => f.Tag.Album == null ? "" : f.Tag.Album)
                        .Where(f => f != "")
                        .Distinct()
                        .ToArray();
                case "Genre":
                    return files.Select(f => f.Tag.FirstGenre == null ? "" : f.Tag.FirstGenre)
                        .Where(f => f != "")
                        .Distinct()
                        .ToArray();
                case "Title":
                    return files.Select(f => f.Tag.Title == null ? "" : f.Tag.Title)
                        .Where(f => f != "")
                        .Distinct()
                        .ToArray();
                case "Year":
                    return files.Select(f => f.Tag.Year.ToString())
                        .Where(f => f != "0")
                        .Distinct()
                        .ToArray();
                default:
                    return result.ToArray();
            }
        }
    }
}
