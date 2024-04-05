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
            repository = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
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

            CboMetadataList.Items.AddRange(new string[]
            {
                "Title",
            });
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

            //string folder = CboPath.Text;
            //CboPath.Items.Clear();
            foreach (var item in CboPath.Items)
            {
                if (item.ToString() != repository)
                {
                    CboPath.Items.Remove(item);
                }
            }
            CboPath.Items.AddRange(Directory.GetDirectories(repository));

            CboPath.EndUpdate();
        }

        private void LsvFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem target = LsvFiles.Items[LsvFiles.SelectedIndices[0]];
            if (target.ImageIndex == 0)
            {
                CboPath.Text = target.Text;
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
                CboPath.Text = dialog.SelectedPath;
                RenderFileTree(CboPath.Text);
            }
        }

        private void BtnUpperFolder_Click(object sender, EventArgs e)
        {
            DirectoryInfo? parent = Directory.GetParent(repository);
            if (parent != null)
            {
                CboPath.Text = parent.FullName;
                RenderFileTree(CboPath.Text);
            }
        }

        private void CboMetadataList_IndexChanged(object sender, EventArgs e)
        {
            List<TagLib.File> files = LsvFiles.Items.OfType<ListViewItem>()
                .Where(i => i.ImageIndex != 0
                && TagLib.SupportedMimeType.AllExtensions.Contains(i.Text.Substring(i.Text.LastIndexOf('.') + 1)))
                .Select(i => TagLib.File.Create(i.Text))
                .ToList();

            LstMetadataValues.Items.AddRange(files.OrderBy(f => f.Tag.Track).Select(f => f.Tag.Title).ToArray());
        }

        //
        // Various utility functions
        //
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ActiveControl == CboPath && keyData == Keys.Enter)
            {
                RenderFileTree(CboPath.Text);
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
                LsvFiles.Items.AddRange(Directory.GetDirectories(path).Select(d => new ListViewItem(d, 0)).ToArray());
                LsvFiles.Items.AddRange(Directory.GetFiles(path).Select(f => new ListViewItem(f, 1)).ToArray());
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

        }

        private void UpdateRepository(string path)
        {
            repository = path;
        }
    }
}
