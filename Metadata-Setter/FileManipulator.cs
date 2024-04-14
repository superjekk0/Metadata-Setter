// Program written by Charles Mandziuk, (c) 2024
//
// Logiciel écrit par Charles Mandziuk, (c) 2024

using Metadata_Setter.Models;
using System.ComponentModel;
using System.Reflection;

namespace Metadata_Setter
{
    public partial class FrmFileManipulator : Form
    {
        private string repository = "";
        private List<FileDisplay> files = new List<FileDisplay>();
        public FrmFileManipulator()
        {
            // TODO : Localize the Winform
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
            // This part will be usefull for translation
            CboMetadataList.DataSource = Enum.GetValues(typeof(TagName))
                .Cast<TagName>()
                .Select(t => new TagDisplay
                {
                    Description = (Attribute.GetCustomAttribute(t.GetType().GetField(t.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description ?? t.ToString()
                        , Value = t.ToString()
                }
                )
                .ToArray();
            CboMetadataList.DisplayMember = "Description";
            CboMetadataList.ValueMember = "Value";
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
            BtnMetadataChange.Enabled = false;
        }

        private void This_Click(object sender, EventArgs e)
        {
            LsvFiles.SelectedIndices.Clear();
            this.ActiveControl = null;
        }

        //
        // Various utility functions
        //
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ActiveControl == CboMetadataList)
            {
                CboMetadataList.DroppedDown = false;
            }
            else if (ActiveControl == CboPath && keyData == Keys.Enter)
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
                BtnMetadataChange.Enabled = false;
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
            BtnMetadataChange.Enabled = aimedFiles.Count != 0;
        }

        private TagName ExtractTagName(string tag)
        {
            try
            {
                return Enum.Parse<TagName>((CboMetadataList.SelectedItem as TagDisplay).Value);
            }
            catch (Exception)
            {
                return (TagName) (-1);
            }
        }

        private object[] FileTags(List<TagLib.File> files, string tag)
        {
            switch (ExtractTagName(tag))
            {
                case TagName.Album:
                    return files.Select(f => f.Tag.Album == null ? "" : f.Tag.Album)
                        .Where(f => f != "")
                        .Distinct()
                        .ToArray();
                case TagName.AlbumArtists:
                    return files.Select(f => new AttributeArray<string>(f.Tag.AlbumArtists))
                        .Where(f => f.Array.Length != 0)
                        .Distinct()
                        .ToArray();
                case TagName.AmazonID:
                    return files.Select(f => f.Tag.AmazonId == null ? "" : f.Tag.AmazonId)
                        .Where(f => f != "")
                        .Distinct()
                        .ToArray();
                case TagName.Artists:
                    break;
                case TagName.BeatsPerMinute:
                    return files.Select(f => f.Tag.BeatsPerMinute.ToString())
                        .Where(f => f != "0")
                        .Distinct()
                        .ToArray();
                case TagName.Comment:
                    return files.Select(f => f.Tag.Comment == null ? "" : f.Tag.Comment)
                        .Where(f => f != "")
                        .Distinct()
                        .ToArray();
                case TagName.Composers:
                    return files.Select(f => new AttributeArray<string>(f.Tag.Composers))
                        .Where(f => f.Array.Length != 0)
                        .Distinct()
                        .ToArray();
                case TagName.ComposersSort:
                    return files.Select(f => new AttributeArray<string>(f.Tag.ComposersSort))
                        .Where(f => f.Array.Length != 0)
                        .Distinct()
                        .ToArray();
                case TagName.Conductor:
                    return files.Select(f => f.Tag.Conductor == null ? "" : f.Tag.Conductor)
                        .Where(f => f != "")
                        .Distinct()
                        .ToArray();
                case TagName.Copyright:
                    return files.Select(f => f.Tag.Copyright == null ? "" : f.Tag.Copyright)
                        .Where(f => f != "")
                        .Distinct()
                        .ToArray();
                case TagName.Description:
                    break;
                case TagName.Disc:
                    break;
                case TagName.DiscCount:
                    break;
                case TagName.Genre:
                    return files.Select(f => new AttributeArray<string>(f.Tag.Genres))
                        .Where(f => f.Array.Length != 0)
                        .Distinct()
                        .ToArray();
                case TagName.Grouping:
                    break;
                case TagName.InitialKey:
                    break;
                case TagName.ISRC:
                    break;
                case TagName.Lenght:
                    break;
                case TagName.Lyrics:
                    break;
                case TagName.Performers:
                    break;
                case TagName.PerformersSort:
                    break;
                case TagName.PerformersRole:
                    break;
                case TagName.Pictures:
                    break;
                case TagName.Publisher:
                    break;
                case TagName.RemixedBy:
                    break;
                case TagName.Subtitle:
                    break;
                case TagName.Title:
                    return files.Select(f => f.Tag.Title == null ? "" : f.Tag.Title)
                        .Where(f => f != "")
                        .Distinct()
                        .ToArray();
                case TagName.TitleSort:
                    break;
                case TagName.Track:
                    break;
                case TagName.TrackCount:
                    break;
                case TagName.Year:
                    return files.Select(f => f.Tag.Year.ToString())
                        .Where(f => f != "0")
                        .Distinct()
                        .ToArray();
                default:
                    return Array.Empty<object>();
            }
            return Array.Empty<object>();
        }

        private void EditFile(TagLib.File file)
        {
            switch (ExtractTagName(CboMetadataList.Text))
            {
                case TagName.Album:
                    file.Tag.Album = TxtApplyValue.Text;
                    break;
                case TagName.AlbumArtists:
                    file.Tag.AlbumArtists = TxtApplyValue.Text.Split(';');
                    break;
                case TagName.AmazonID:
                    file.Tag.AmazonId = TxtApplyValue.Text;
                    break;
                case TagName.Artists:
                    break;
                case TagName.BeatsPerMinute:
                    file.Tag.BeatsPerMinute = (uint)NumNumberValues.Value;
                    break;
                case TagName.Comment:
                    file.Tag.Comment = TxtApplyValue.Text;
                    break;
                case TagName.Composers:
                    file.Tag.Composers = TxtApplyValue.Text.Split(';');
                    break;
                case TagName.ComposersSort:
                    file.Tag.ComposersSort = TxtApplyValue.Text.Split(';');
                    break;
                case TagName.Conductor:
                    file.Tag.Conductor = TxtApplyValue.Text;
                    break;
                case TagName.Copyright:
                    file.Tag.Copyright = TxtApplyValue.Text;
                    break;
                case TagName.Description:
                    break;
                case TagName.Disc:
                    break;
                case TagName.DiscCount:
                    break;
                case TagName.Genre:
                    file.Tag.Genres = TxtApplyValue.Text.Split(';');
                    break;
                case TagName.Grouping:
                    break;
                case TagName.InitialKey:
                    break;
                case TagName.ISRC:
                    break;
                case TagName.Lenght:
                    break;
                case TagName.Lyrics:
                    break;
                case TagName.Performers:
                    break;
                case TagName.PerformersSort:
                    break;
                case TagName.PerformersRole:
                    break;
                case TagName.Pictures:
                    break;
                case TagName.Publisher:
                    break;
                case TagName.RemixedBy:
                    break;
                case TagName.Subtitle:
                    break;
                case TagName.Title:
                    file.Tag.Title = TxtApplyValue.Text;
                    break;
                case TagName.TitleSort:
                    break;
                case TagName.Track:
                    break;
                case TagName.TrackCount:
                    break;
                case TagName.Year:
                    file.Tag.Year = (uint)NumNumberValues.Value;
                    break;
                default:
                    break;
            }
        }

        private bool ValidInput()
        {
            switch (ExtractTagName(CboMetadataList.Text))
            {
                case TagName.Album:
                case TagName.AlbumArtists:
                case TagName.AmazonID:
                case TagName.Artists:
                    return true;
                case TagName.BeatsPerMinute:
                    return NumNumberValues.Value >= 0 && NumNumberValues.Value <= 500;
                case TagName.Comment:
                case TagName.Composers:
                case TagName.ComposersSort:
                case TagName.Conductor:
                case TagName.Copyright:
                    return true;
                case TagName.Description:
                case TagName.Disc:
                    break;
                case TagName.DiscCount:
                    break;
                case TagName.Genre:
                    return true;
                case TagName.Grouping:
                    break;
                case TagName.InitialKey:
                    break;
                case TagName.ISRC:
                    break;
                case TagName.Lenght:
                    break;
                case TagName.Lyrics:
                    break;
                case TagName.Performers:
                    break;
                case TagName.PerformersSort:
                    break;
                case TagName.PerformersRole:
                    break;
                case TagName.Pictures:
                    break;
                case TagName.Publisher:
                    break;
                case TagName.RemixedBy:
                    break;
                case TagName.Subtitle:
                    break;
                case TagName.Title:
                    break;
                case TagName.TitleSort:
                    break;
                case TagName.Track:
                    break;
                case TagName.TrackCount:
                    break;
                case TagName.Year:
                    return NumNumberValues.Value >= 0 && NumNumberValues.Value <= 9999;
                default:
                    return false;
            }
            return false;
        }

        private void DisplayMetadataContext()
        {
            switch (ExtractTagName(CboMetadataList.Text))
            {
                case TagName.Album:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = CboMetadataList.Text;
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.AlbumArtists:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Artist1;Artist2";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.AmazonID:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Amazon ID";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Artists:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Artist1;Artist2";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.BeatsPerMinute:
                    TxtApplyValue.Visible = false;
                    NumNumberValues.Minimum = 0;
                    NumNumberValues.Maximum = 500;
                    NumNumberValues.Value = 120;
                    NumNumberValues.Visible = true;
                    break;
                case TagName.Comment:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "This is a sample comment";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Composers:
                case TagName.ComposersSort:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Composer1;Composer2";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Conductor:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "John Smith";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Copyright:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Record label company";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Description:
                    break;
                case TagName.Disc:
                    break;
                case TagName.DiscCount:
                    break;
                case TagName.Genre:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Genre1;Genre2";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Grouping:
                    break;
                case TagName.InitialKey:
                    break;
                case TagName.ISRC:
                    break;
                case TagName.Lenght:
                    break;
                case TagName.Lyrics:
                    break;
                case TagName.Performers:
                    break;
                case TagName.PerformersSort:
                    break;
                case TagName.PerformersRole:
                    break;
                case TagName.Pictures:
                    break;
                case TagName.Publisher:
                    break;
                case TagName.RemixedBy:
                    break;
                case TagName.Subtitle:
                    break;
                case TagName.Title:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = CboMetadataList.Text;
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.TitleSort:
                    break;
                case TagName.Track:
                    break;
                case TagName.TrackCount:
                    break;
                case TagName.Year:
                    NumNumberValues.Visible = true;
                    NumNumberValues.Minimum = 0;
                    NumNumberValues.Maximum = 9999;
                    NumNumberValues.Value = 2000;
                    TxtApplyValue.Visible = false;
                    break;
                default:
                    break;
            }
        }
    }
}
