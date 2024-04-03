namespace Metadata_Setter
{
    public partial class FrmFileManipulator : Form
    {
        public FrmFileManipulator()
        {
            InitializeComponent();
        }

        private void MnuOptionsFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CboMetadataList_CursorChanged(object sender, EventArgs e)
        {
            this.Cursor = this.DefaultCursor;
        }
    }
}
