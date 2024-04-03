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
            CboPath.Text = folder;
            //CboPath.Items.AddRange(Directory.GetFiles(folder));
            CboPath.Items.AddRange(Directory.GetDirectories(folder).OrderBy(f => f).ToArray());
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
            LstFiles.Items.Clear();
            LstFiles.Items.AddRange(Directory.GetDirectories(folder));
            LstFiles.Items.AddRange(Directory.GetFiles(folder));

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

        //
        // Various utility functions
        //
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ActiveControl == CboPath && keyData == Keys.Enter)
            {
                CboPath_ValueChanged(null, null);
                LstFiles.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
