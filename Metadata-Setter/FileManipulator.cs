// Program written by Charles Mandziuk, (c) 2024
//
// Logiciel écrit par Charles Mandziuk, (c) 2024


namespace Metadata_Setter
{
    public partial class FrmFileManipulator : Form
    {
        public FrmFileManipulator()
        {
            InitializeComponent();
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //CboPath.Items.AddRange(Directory.GetFiles(folder));

            ColumnHeader header = new ColumnHeader();
            header.Text = "";
            header.Name = "Type";
            header.Width = LsvFiles.Width;

            LsvFiles.Columns.Add(header);
            LsvFiles.HeaderStyle = ColumnHeaderStyle.None;

            CboPath.Items.Add(folder);
            CboPath.Items.AddRange(Directory.GetDirectories(folder).OrderBy(f => f).ToArray());
            CboPath.Text = folder;
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
            CboPath.BeginUpdate();

            string folder = CboPath.Text;

            LsvFiles.Items.Clear();
            LsvFiles.Items.AddRange(Directory.GetDirectories(folder).Select(d => new ListViewItem(d, 0)).ToArray());
            LsvFiles.Items.AddRange(Directory.GetFiles(folder).Select(f => new ListViewItem(f, 1)).ToArray());

            CboPath.EndUpdate();
        }

        private void CboPath_DropDown(object sender, EventArgs e)
        {
            CboPath.BeginUpdate();

            string folder = CboPath.Text;
            CboPath.Items.Clear();
            CboPath.Items.Add(folder);
            CboPath.Items.AddRange(Directory.GetDirectories(folder));
            CboPath.Text = folder;

            CboPath.EndUpdate();
        }

        private void LsvFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem target = LsvFiles.Items[LsvFiles.SelectedIndices[0]];
            if (target.ImageIndex == 0)
            {
                CboPath.Text = target.Text;
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
                RenderFileTree();
                LsvFiles.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void RenderFileTree()
        {
            CboPath.BeginUpdate();

            string folder = CboPath.Text;

            LsvFiles.Items.Clear();
            LsvFiles.Items.AddRange(Directory.GetDirectories(folder).Select(d => new ListViewItem(d, 0)).ToArray());
            LsvFiles.Items.AddRange(Directory.GetFiles(folder).Select(f => new ListViewItem(f, 1)).ToArray());



            CboPath.EndUpdate();
        }
    }
}
