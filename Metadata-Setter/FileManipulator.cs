// Program written by Charles Mandziuk, (c) 2024
//
// Logiciel écrit par Charles Mandziuk, (c) 2024

using Metadata_Setter.Models;
using System.ComponentModel;
using System.Reflection;
using TagLib;

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

            LsvFiles.Columns.Add("", "Type", LsvFiles.Width);
            LsvFiles.HeaderStyle = ColumnHeaderStyle.None;

            lsvMetadataValues.Columns.Add("Data", "Data", lsvMetadataValues.Width);
            lsvMetadataValues.Columns.Add("Refers", "ReferenceInfo", 0);
            lsvMetadataValues.HeaderStyle = ColumnHeaderStyle.None;

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

            //LstMetadataValues.Items.Clear();
            lsvMetadataValues.Items.Clear();

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

            //LstMetadataValues.Items.AddRange(FileTags(aimedFiles, CboMetadataList.Text));
            lsvMetadataValues.Items.AddRange(FileTags(aimedFiles, CboMetadataList.Text)
                .Select(f => new ListViewItem(f.ToString())).ToArray());
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
                    return files.Where(f => f.Tag.Album != null)
                        .Select(f => f.Tag.Album)
                        .Distinct()
                        .ToArray();
                case TagName.AlbumArtists:
                    return files.Where(f => f.Tag.AlbumArtists.Length != 0)
                        .Select(f => new AttributeArray<string>(f.Tag.AlbumArtists))
                        .Distinct()
                        .ToArray();
                case TagName.AmazonID:
                    return files.Where(f => f.Tag.AmazonId != null)
                        .Select(f => f.Tag.AmazonId)
                        .Distinct()
                        .ToArray();
                case TagName.Artists:
                    break;
                case TagName.BeatsPerMinute:
                    return files.Where(f => f.Tag.BeatsPerMinute != 0)
                        .Select(f => f.Tag.BeatsPerMinute.ToString())
                        .Distinct()
                        .ToArray();
                case TagName.Comment:
                    return files.Where(f => f.Tag.Comment != null)
                        .Select(f => f.Tag.Comment)
                        .Distinct()
                        .ToArray();
                case TagName.Composers:
                    return files.Where(f => f.Tag.Composers.Length != 0)
                        .Select(f => new AttributeArray<string>(f.Tag.Composers))
                        .Distinct()
                        .ToArray();
                case TagName.ComposersSort:
                    return files.Where(f => f.Tag.ComposersSort.Length != 0)
                        .Select(f => new AttributeArray<string>(f.Tag.ComposersSort))
                        .Distinct()
                        .ToArray();
                case TagName.Conductor:
                    return files.Where(f => f.Tag.Conductor != null)
                        .Select(f => f.Tag.Conductor)
                        .Distinct()
                        .ToArray();
                case TagName.Copyright:
                    return files.Where(f => f.Tag.Copyright != null)
                        .Select(f => f.Tag.Copyright)
                        .Distinct()
                        .ToArray();
                case TagName.Description:
                    return files.Where(f => f.Tag.Description != null)
                        .Select(f => f.Tag.Description)
                        .Distinct()
                        .ToArray();
                case TagName.Disc:
                    return files.Where(f => f.Tag.Disc != 0)
                        .Select(f => f.Tag.Disc.ToString())
                        .Distinct()
                        .ToArray();
                case TagName.DiscCount:
                    return files.Where(f => f.Tag.DiscCount != 0)
                        .Select(f => f.Tag.DiscCount.ToString())
                        .Distinct()
                        .ToArray();
                case TagName.Genre:
                    return files.Where(f => f.Tag.Genres.Length != 0)
                        .Select(f => new AttributeArray<string>(f.Tag.Genres))
                        .Distinct()
                        .ToArray();
                case TagName.Grouping:
                    return files.Where(f => f.Tag.Grouping != null)
                        .Select(f => f.Tag.Grouping)
                        .Distinct()
                        .ToArray();
                case TagName.InitialKey:
                    return files.Where(f => f.Tag.InitialKey != null)
                        .Select(f => f.Tag.InitialKey)
                        .Distinct()
                        .ToArray();
                case TagName.ISRC:
                    return files.Where(f => f.Tag.ISRC != null)
                        .Select(f => f.Tag.ISRC)
                        .Distinct()
                        .ToArray();
                case TagName.Lyrics:
                    return files.Where(f => f.Tag.Lyrics != null)
                        .Select(f => f.Tag.Lyrics)
                        .Distinct()
                        .ToArray();
                case TagName.Performers:
                    return files.Where(a => a.Tag.Performers.Length != 0)
                        .Select(f => new AttributeArray<string>(f.Tag.Performers))
                        .Distinct()
                        .ToArray();
                case TagName.PerformersSort:
                    return files.Where(a => a.Tag.PerformersSort.Length != 0)
                        .Select(f => new AttributeArray<string>(f.Tag.PerformersSort))
                        .Distinct()
                        .ToArray();
                case TagName.PerformersRole:
                    return files.Where(a => a.Tag.PerformersRole.Length != 0)
                        .Select(f => new AttributeArray<string>(f.Tag.PerformersRole))
                        .Distinct()
                        .ToArray();
                //case TagName.Pictures:
                //    return files.Select(f => new AttributeArray<IPicture>(f.Tag.Pictures))
                //        .Where(f => f.Array.Length != 0)
                //        .Distinct()
                //        .ToArray();
                case TagName.Publisher:
                    return files.Where(f => f.Tag.Publisher != null)
                        .Select(f => f.Tag.Publisher)
                        .Distinct()
                        .ToArray();
                case TagName.RemixedBy:
                    return files.Where(f => f.Tag.RemixedBy != null)
                        .Select(f => f.Tag.RemixedBy)
                        .Distinct()
                        .ToArray();
                case TagName.Subtitle:
                    return files.Where(f => f.Tag.Subtitle != null)
                        .Select(f => f.Tag.Subtitle)
                        .Distinct()
                        .ToArray();
                case TagName.Title:
                    return files.Where(f => f.Tag.Title != null)
                        .Select(f => f.Tag.Title)
                        .Distinct()
                        .ToArray();
                case TagName.TitleSort:
                    return files.Where(f => f.Tag.TitleSort != null)
                        .Select(f => f.Tag.TitleSort)
                        .Distinct()
                        .ToArray();
                case TagName.Track:
                    return files.Where(f => f.Tag.Track != 0)
                        .Select(f => f.Tag.Track.ToString())
                        .Distinct()
                        .ToArray();
                case TagName.TrackCount:
                    return files.Where(f => f.Tag.TrackCount != 0)
                        .Select(f => f.Tag.TrackCount.ToString())
                        .Distinct()
                        .ToArray();
                case TagName.Year:
                    return files.Where(f => f.Tag.Year != 0)
                        .Select(f => f.Tag.Year.ToString())
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
                    file.Tag.Description = TxtApplyValue.Text;
                    break;
                case TagName.Disc:
                    file.Tag.Disc = (uint)NumNumberValues.Value;
                    break;
                case TagName.DiscCount:
                    file.Tag.DiscCount = (uint)NumNumberValues.Value;
                    break;
                case TagName.Genre:
                    file.Tag.Genres = TxtApplyValue.Text.Split(';');
                    break;
                case TagName.Grouping:
                    file.Tag.Grouping = TxtApplyValue.Text;
                    break;
                case TagName.InitialKey:
                    file.Tag.InitialKey = TxtApplyValue.Text;
                    break;
                case TagName.ISRC:
                    file.Tag.ISRC = TxtApplyValue.Text;
                    break;
                case TagName.Lyrics:
                    file.Tag.Lyrics = TxtApplyValue.Text;
                    break;
                case TagName.Performers:
                    file.Tag.Performers = TxtApplyValue.Text.Split(';');
                    break;
                case TagName.PerformersSort:
                    file.Tag.PerformersSort = TxtApplyValue.Text.Split(';');
                    break;
                case TagName.PerformersRole:
                    file.Tag.PerformersRole = TxtApplyValue.Text.Split(';');
                    break;
                //case TagName.Pictures:
                //    // TODO : Set a specific case for pictures
                //    break;
                case TagName.Publisher:
                    file.Tag.Publisher = TxtApplyValue.Text;
                    break;
                case TagName.RemixedBy:
                    file.Tag.RemixedBy = TxtApplyValue.Text;
                    break;
                case TagName.Subtitle:
                    file.Tag.Subtitle = TxtApplyValue.Text;
                    break;
                case TagName.Title:
                    file.Tag.Title = TxtApplyValue.Text;
                    break;
                case TagName.TitleSort:
                    file.Tag.TitleSort = TxtApplyValue.Text;
                    break;
                case TagName.Track:
                    file.Tag.Track = (uint)NumNumberValues.Value;
                    break;
                case TagName.TrackCount:
                    file.Tag.TrackCount = (uint)NumNumberValues.Value;
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
                case TagName.Description:
                    return true;
                case TagName.Disc:
                case TagName.DiscCount:
                    return NumNumberValues.Value >= 0 && NumNumberValues.Value <= NumNumberValues.Maximum;
                case TagName.Genre:
                case TagName.Grouping:
                case TagName.InitialKey:
                case TagName.ISRC:
                case TagName.Lyrics:
                case TagName.Performers:
                case TagName.PerformersSort:
                case TagName.PerformersRole:
                    return true;
                //case TagName.Pictures:
                //    // TODO : Set a specific case for pictures
                //    return false;
                case TagName.Publisher:
                case TagName.RemixedBy:
                case TagName.Subtitle:
                case TagName.Title:
                case TagName.TitleSort:
                    return true;
                case TagName.Track:
                case TagName.TrackCount:
                    return NumNumberValues.Value >= 0 && NumNumberValues.Value <= NumNumberValues.Maximum;
                case TagName.Year:
                    return NumNumberValues.Value >= 0 && NumNumberValues.Value <= 9999;
                default:
                    return false;
            }
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
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "This is a sample description";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Disc:
                case TagName.DiscCount:
                    // The maximum will most likely never be attained
                    NumNumberValues.Visible = true;
                    NumNumberValues.Minimum = 0;
                    NumNumberValues.Maximum = 1000;
                    NumNumberValues.Value = 1;
                    TxtApplyValue.Visible = false;
                    break;
                case TagName.Genre:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Genre1;Genre2";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Grouping:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Group description";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.InitialKey:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "C#m";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.ISRC:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "AB-CD1-23-45678";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Lyrics:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "This is a sample Lyrics";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Performers:
                case TagName.PerformersSort:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Performer1;Performer2";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.PerformersRole:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Piano;Bass";
                    TxtApplyValue.Visible = true;
                    break;
                //case TagName.Pictures:
                //    break;
                case TagName.Publisher:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Publisher";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.RemixedBy:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Remixer";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Subtitle:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "This is a Subtitle";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Title:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = CboMetadataList.Text;
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.TitleSort:
                    NumNumberValues.Visible = false;
                    TxtApplyValue.PlaceholderText = "Title";
                    TxtApplyValue.Visible = true;
                    break;
                case TagName.Track:
                case TagName.TrackCount:
                    NumNumberValues.Visible = true;
                    NumNumberValues.Minimum = 0;
                    NumNumberValues.Maximum = 1000;
                    NumNumberValues.Value = 1;
                    TxtApplyValue.Visible = false;
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
