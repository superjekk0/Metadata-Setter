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
            //CboPath.Items.AddRange(Directory.GetFiles(folder));

            ColumnHeader header = new ColumnHeader();
            header.Text = "";
            header.Name = "Type";
            header.Width = LsvFiles.Width;

            LsvFiles.Columns.Add(header);
            LsvFiles.HeaderStyle = ColumnHeaderStyle.None;

            CboPath.Items.Add(repository);
            CboPath.Items.AddRange(Directory.GetDirectories(repository).OrderBy(f => f).ToArray());
            CboPath.Text = repository;
        }

        private void MnuOptionsFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        //
        // CboPath
        //
        private void CboPath_ValueChanged(object sender, EventArgs e)
        {
            RenderFileTree();
        }

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
            //CboPath.Items.Add(folder);
            CboPath.Items.AddRange(Directory.GetDirectories(repository));
            //CboPath.Text = folder;

            CboPath.EndUpdate();
        }

        private void LsvFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem target = LsvFiles.Items[LsvFiles.SelectedIndices[0]];
            if (target.ImageIndex == 0)
            {
                CboPath.Text = target.Text;
                UpdateRepository();
                RenderFileTree();
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
                RenderFileTree();
            }
        }

        private void BtnUpperFolder_Click(object sender, EventArgs e)
        {
            DirectoryInfo? parent = Directory.GetParent(repository);
            if (parent != null)
            {
                //repository = parent.FullName;
                CboPath.Text = parent.FullName;
                UpdateRepository();
                RenderFileTree();
            }
        }


        //
        // Various utility functions
        //
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ActiveControl == CboPath && keyData == Keys.Enter)
            {
                UpdateRepository();
                RenderFileTree();
                LsvFiles.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void RenderFileTree()
        {
            CboPath.BeginUpdate();

            LsvFiles.Items.Clear();
            LsvFiles.Items.AddRange(Directory.GetDirectories(repository).Select(d => new ListViewItem(d, 0)).ToArray());
            LsvFiles.Items.AddRange(Directory.GetFiles(repository).Select(f => new ListViewItem(f, 1)).ToArray());

            CboPath.EndUpdate();
        }

        private void UpdateRepository()
        {
            repository = CboPath.Text;
        }

    }
}
