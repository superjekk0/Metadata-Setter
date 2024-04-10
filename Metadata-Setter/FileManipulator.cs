// Program written by Charles Mandziuk, (c) 2024
//
// Logiciel écrit par Charles Mandziuk, (c) 2024

using System.Text.RegularExpressions;

namespace Metadata_Setter
{
    public partial class FrmFileManipulator : Form
    {
        private string repository = "";
        private List<FileDisplay> files = new List<FileDisplay>();
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
            try
            {
                CboPath.Items.AddRange(Directory.GetDirectories(repository).Select(d => d += '\\').ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Directory access", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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
                CboPath.Text = dialog.SelectedPath + (dialog.SelectedPath.LastIndexOf('\\') == dialog.SelectedPath.Length - 1 ? "" : "\\");
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
            DisplayMetadataContext();
        }

        private void LsvFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            PrgModificationApply.Maximum = (LsvFiles.SelectedIndices.Count == 0 ? files.Count : LsvFiles.SelectedIndices.Count);
            UpdateTagList(LsvFiles.SelectedIndices);
        }

        private void BtnMetadataChange_Click(object sender, EventArgs e)
        {
            if (CboMetadataList.SelectedIndex == -1)
            {
                return;
            }

            if (!ValidInput())
            {
                MessageBox.Show(string.Format("'{0}' is not a valid value for '{1}'", TxtApplyValue.Text, CboMetadataList.Text), "Wrong value", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            this.Enabled = false;
            List<TagLib.File> aimedFiles = new List<TagLib.File>();
            if (LsvFiles.SelectedIndices.Count != 0)
            {
                aimedFiles = files
                    .Where(f => LsvFiles.SelectedIndices.Contains(f.Index))
                    .Select(f => f.File)
                    .ToList();
            }
            else
            {
                aimedFiles = files.Select(f => f.File).ToList();
            }
            // TODO : Parallelize the modification process while being capable of
            // updating the progress bar
            //Task.WaitAll(files.Select(async f => await Task.Run(() =>
            //{
            //    f.File.Tag.Year = uint.Parse(TxtApplyValue.Text);
            //    f.File.Save();
            //    Thread.Sleep(1000);
            //})).ToArray());
            foreach (TagLib.File file in aimedFiles)
            {
                EditFile(file);
                ++PrgModificationApply.Value;
            }

            aimedFiles.ForEach(f => f.Save());
            MessageBox.Show("All files have been modified.", "Modification complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            PrgModificationApply.Value = PrgModificationApply.Minimum;
            UpdateTagList(LsvFiles.SelectedIndices);
            this.Enabled = true;
        }

        private void This_Click(object sender, EventArgs e)
        {
            LsvFiles.SelectedIndices.Clear();
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
                // It should not be necessary to make a copy of files, as if the directory
                // does not exist, the program will already have thrown an exception.
                LsvFiles.Items.Clear();
                LsvFiles.Items.AddRange(Directory.GetDirectories(path).Select(d => new ListViewItem(d.Substring(d.LastIndexOf('\\') + 1), 0)).ToArray());
                LsvFiles.Items.AddRange(Directory.GetFiles(path).Select(f => new ListViewItem(f.Substring(f.LastIndexOf('\\') + 1), 1)).ToArray());
                files = LsvFiles.Items.OfType<ListViewItem>()
                    .Where(i => i.ImageIndex != 0 && TagLib.SupportedMimeType
                    .AllExtensions.Contains(i.Text.Substring(i.Text.LastIndexOf('.') + 1)))
                    .Select(i => new FileDisplay(TagLib.File.Create(path + i.Text), i.Index))
                    .ToList();
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
            PrgModificationApply.Maximum = files.Count;
        }

        private void UpdateTagList(ListView.SelectedIndexCollection selectedIndices)
        {
            if (CboMetadataList.SelectedIndex == -1)
            {
                return;
            }

            LstMetadataValues.Items.Clear();

            List<TagLib.File> aimedFiles;
            if (selectedIndices.Count != 0)
            {
                aimedFiles = files
                    .Where(f => selectedIndices.Contains(f.Index))
                    .Select(f => f.File)
                    .ToList();
            }
            else
            {
                aimedFiles = files.Select(f => f.File).ToList();
            }

            LstMetadataValues.Items.AddRange(FileTags(aimedFiles, CboMetadataList.Text));
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

        private void EditFile(TagLib.File file)
        {
            switch (CboMetadataList.Text)
            {
                case "Title":
                    file.Tag.Title = TxtApplyValue.Text;
                    break;
                case "Album":
                    file.Tag.Album = TxtApplyValue.Text;
                    break;
                case "Genre":
                    file.Tag.Genres = new string[] { TxtApplyValue.Text };
                    break;
                case "Year":
                    file.Tag.Year = (uint) NumNumberValues.Value;
                    break;
                default:
                    break;
            }
        }

        private bool ValidInput()
        {
            switch (CboMetadataList.Text)
            {
                case "Title":
                case "Album":
                case "Genre":
                    return true;
                case "Year":
                    return NumNumberValues.Value >= 0 && NumNumberValues.Value <= 9999;
                default:
                    break;

            }
            return false;
        }

        private void DisplayMetadataContext()
        {
            switch (CboMetadataList.Text)
            {
                case "Title":
                case "Album":
                case "Genre":
                    NumNumberValues.Visible = false;
                    TxtApplyValue.Visible = true;
                    break;
                case "Year":
                    NumNumberValues.Visible = true;
                    NumNumberValues.Minimum = 0;
                    NumNumberValues.Maximum = 9999;
                    NumNumberValues.Value = 2000;
                    TxtApplyValue.Visible = false;
                    break;
            }
        }
    }
}
