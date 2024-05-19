// Program written by Charles Mandziuk, (c) 2024
//
// Logiciel écrit par Charles Mandziuk, (c) 2024

using Metadata_Setter.Models;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text.RegularExpressions;
using TagLib;

namespace Metadata_Setter
{
    public partial class FrmFileManipulator : Form
    {
        private string repository = "";
        private List<FileDisplay> files = new List<FileDisplay>();
        private TagName tagName;
        ListViewItem? targetedItem = null;
        ListViewItem? hoveredItem = null;
        private bool dragging = false;
        private readonly Cursor listCursor = new Cursor("list.ico");
        private readonly Cursor grabCursor = new Cursor("grab.ico");

        public FrmFileManipulator()
        {
            // TODO : Localize the Winform
            InitializeComponent();
            repository = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + '\\';

            lsvFiles.Columns.Add("", "Type", lsvFiles.Width);
            lsvFiles.HeaderStyle = ColumnHeaderStyle.None;

            lsvMetadataValues.Columns.Add("Data", "Data", lsvMetadataValues.Width);
            lsvMetadataValues.Columns.Add("Refers", "ReferenceInfo", 0);
            lsvMetadataValues.HeaderStyle = ColumnHeaderStyle.None;

            cboPath.Items.Add(repository);
            cboPath.Items.AddRange(Directory.GetDirectories(repository).OrderBy(f => f).ToArray());
            cboPath.Text = repository;

            RenderFileTree(cboPath.Text);
            // This part will be usefull for translation
            cboMetadataList.DataSource = Enum.GetValues(typeof(TagName))
                .Cast<TagName>()
                .Select(t => new TagDisplay
                {
                    Description = (Attribute.GetCustomAttribute(t.GetType().GetField(t.ToString())!, typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description ?? t.ToString()
                        ,
                    Value = t.ToString()
                }
                )
                .ToArray();
            cboMetadataList.DisplayMember = "Description";
            cboMetadataList.ValueMember = "Value";
        }

        private void MnuOptionsFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CboPath_DropDown(object sender, EventArgs e)
        {
            cboPath.BeginUpdate();

            foreach (var item in cboPath.Items)
            {
                if (item.ToString() != repository)
                {
                    cboPath.Items.Remove(item);
                }
            }
            try
            {
                cboPath.Items.AddRange(Directory.GetDirectories(repository).Select(d => d += '\\').ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Directory access", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            cboPath.EndUpdate();
        }

        private void LsvFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem target = lsvFiles.Items[lsvFiles.SelectedIndices[0]];
            if (target.ImageIndex == 0)
            {
                cboPath.Text += target.Text + '\\';

                RenderFileTree(cboPath.Text);
            }
        }

        private void BtnFolderSearch_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog()
            {
                InitialDirectory = cboPath.Text
            };

            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                cboPath.Text = dialog.SelectedPath + (dialog.SelectedPath.LastIndexOf('\\') == dialog.SelectedPath.Length - 1 ? "" : "\\");
                RenderFileTree(cboPath.Text);
            }
        }

        private void BtnUpperFolder_Click(object sender, EventArgs e)
        {
            DirectoryInfo? parent = Directory.GetParent(repository);
            if (parent != null && parent.Parent != null)
            {
                cboPath.Text = parent.Parent.FullName + (parent.Root.FullName == parent.Parent.FullName ? "" : "\\");
                RenderFileTree(cboPath.Text);
            }
        }

        private void CboMetadataList_IndexChanged(object sender, EventArgs e)
        {
            UpdateTagList(lsvFiles.SelectedIndices);
        }

        private void LsvFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvMetadataValues.SelectedIndices.Count == 0)
            {
                prgModificationApply.Maximum = (lsvFiles.SelectedIndices.Count == 0 ? files.Count : lsvFiles.SelectedIndices.Count);
                UpdateTagList(lsvFiles.SelectedIndices);
            }
            else
            {
                
            }
        }

        private void BtnMetadataChange_Click(object sender, EventArgs e)
        {
            if (cboMetadataList.SelectedIndex == -1)
            {
                return;
            }

            if (!ValidInput())
            {
                MessageBox.Show(string.Format("'{0}' is not a valid value for '{1}'", txtApplyValue.Text, cboMetadataList.Text), "Wrong value", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            this.Enabled = false;
            List<TagLib.File> aimedFiles = new List<TagLib.File>();
            if (lsvFiles.SelectedIndices.Count != 0)
            {
                aimedFiles = files
                    .Where(f => lsvFiles.SelectedIndices.Contains(f.Index))
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
                ++prgModificationApply.Value;
            }

            aimedFiles.ForEach(f => f.Save());
            MessageBox.Show("All files have been modified.", "Modification complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            prgModificationApply.Value = prgModificationApply.Minimum;
            UpdateTagList(lsvFiles.SelectedIndices);
            this.Enabled = true;
            btnMetadataChange.Enabled = false;
        }

        private void This_Click(object sender, EventArgs e)
        {
            lsvFiles.SelectedIndices.Clear();
            lsvMetadataValues.SelectedIndices.Clear();
            this.ActiveControl = null;
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            if (lsvMetadataValues.SelectedIndices.Count == 0)
            {
                return;
            }

            ListViewItem? item = lsvMetadataValues.SelectedItems[0];
            if (item != null && item.Index > 0)
            {
                item.Selected = false;
                item.Focused = false;
                AutoScrollListView(item.Index - 1);
            }
        }

        private void BtnDown_Click(object sender, EventArgs e)
        {
            if (lsvMetadataValues.SelectedIndices.Count == 0)
            {
                return;
            }

            ListViewItem? item = lsvMetadataValues.SelectedItems[0];
            if (item != null && item.Index < lsvMetadataValues.Items.Count - 1)
            {
                item.Selected = false;
                item.Focused = false;
                AutoScrollListView(item.Index + 1);
            }
        }

        private void BtnHigher_Click(object sender, EventArgs e)
        {
            if (lsvMetadataValues.SelectedIndices.Count == 0)
            {
                return;
            }

            ListViewItem? item = lsvMetadataValues.SelectedItems[0];
            if (item != null && item.Index > 0)
            {
                btnMetadataChange.Enabled = true;
                Reorder(item, lsvMetadataValues.Items[item.Index - 1]);
                item.Selected = false;
                item.Focused = false;
                AutoScrollListView(item.Index - 1);
            }
        }

        private void BtnLower_Click(object sender, EventArgs e)
        {
            if (lsvMetadataValues.SelectedIndices.Count == 0)
            {
                return;
            }

            ListViewItem? item = lsvMetadataValues.SelectedItems[0];
            if (item != null && item.Index < lsvMetadataValues.Items.Count - 1)
            {
                btnMetadataChange.Enabled = true;
                Reorder(lsvMetadataValues.Items[item.Index + 1], item);
                item.Selected = false;
                item.Focused = false;
                AutoScrollListView(item.Index + 1);
            }
        }

        private void LsvMetadataValues_MouseMove(object sender, MouseEventArgs e)
        {
            if (tagName != TagName.Track)
            {
                return;
            }

            if (!dragging)
            {
                targetedItem = lsvMetadataValues.GetItemAt(e.X, e.Y);
            }
            else
            {
                if (hoveredItem != null && hoveredItem != lsvMetadataValues.GetItemAt(e.X, e.Y))
                {
                    hoveredItem.Selected = false;
                }
                hoveredItem = lsvMetadataValues.GetItemAt(e.X, e.Y);
                if (hoveredItem != null)
                {
                    hoveredItem.Selected = true;
                    if (hoveredItem.Index > 0 && lsvMetadataValues.TopItem == hoveredItem)
                    {
                        lsvMetadataValues.EnsureVisible(lsvMetadataValues.TopItem.Index - 1);
                    }
                    else
                    {
                        hoveredItem.EnsureVisible();
                    }
                }
            }
        }

        private void LsvMetadataValues_MouseUp(object sender, MouseEventArgs e)
        {
            if (tagName != TagName.Track)
            {
                return;
            }

            if (e.Button == MouseButtons.Left && dragging)
            {
                lsvMetadataValues.Cursor = listCursor;
                dragging = false;
                if (targetedItem != null && hoveredItem != null)
                {
                    Reorder(targetedItem, hoveredItem);
                    targetedItem = null;
                    hoveredItem = null;
                }
            }
        }

        private void LsvMetadataValues_MouseDown(object sender, MouseEventArgs e)
        {
            if (tagName != TagName.Track)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                lsvMetadataValues.Cursor = grabCursor;
                dragging = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (tagName != TagName.Track)
            {
                return;
            }

            if (dragging && !lsvMetadataValues.Bounds.Contains(e.Location))
            {
                lsvMetadataValues.Cursor = listCursor;
                dragging = false;
            }
        }

        //
        // Various utility functions
        //
        private void AutoScrollListView(int index)
        {
            lsvMetadataValues.Items[index].Selected = true;
            lsvMetadataValues.Items[index].Focused = true;
            lsvMetadataValues.Items[index].EnsureVisible();
            lsvMetadataValues.Focus();
        }

        private void Reorder(ListViewItem item1, ListViewItem item2)
        {
            btnMetadataChange.Enabled = true;
            // item1 is the beginning
            if (item1.Index < item2.Index)
            {
                string displayToBegin = item2.SubItems[1].Text;
                string refersToBegin = item2.SubItems[2].Text;
                for (int i = item2.Index; i > item1.Index; --i)
                {
                    lsvMetadataValues.Items[i].SubItems[1].Text = lsvMetadataValues.Items[i - 1].SubItems[1].Text;
                    lsvMetadataValues.Items[i].SubItems[2].Text = lsvMetadataValues.Items[i - 1].SubItems[2].Text;
                }
                item1.SubItems[1].Text = displayToBegin;
                item1.SubItems[2].Text = refersToBegin;
            }
            // item2 is the beginning
            else
            {
                string displayToBegin = item1.SubItems[1].Text;
                string refersToBegin = item1.SubItems[2].Text;
                for (int i = item1.Index; i > item2.Index; --i)
                {
                    lsvMetadataValues.Items[i].SubItems[1].Text = lsvMetadataValues.Items[i - 1].SubItems[1].Text;
                    lsvMetadataValues.Items[i].SubItems[2].Text = lsvMetadataValues.Items[i - 1].SubItems[2].Text;
                }
                item2.SubItems[1].Text = displayToBegin;
                item2.SubItems[2].Text = refersToBegin;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ActiveControl == cboMetadataList)
            {
                if (keyData == Keys.Enter)
                {
                    //TxtApplyValue.Focus();
                    UpdateTagList(lsvFiles.SelectedIndices);
                }
                cboMetadataList.DroppedDown = false;
            }
            else if (ActiveControl == cboPath && keyData == Keys.Enter)
            {
                RenderFileTree(cboPath.Text +
                    (cboPath.Text.Length - 1 == cboPath.Text.LastIndexOf('\\') ? "" : "\\"));
                cboPath.Text = repository;
                lsvFiles.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // TODO : Make this method asynchronous
        private void RenderFileTree(string path)
        {
            var itemsBefore = lsvFiles.Items.OfType<ListViewItem>().ToArray();
            cboPath.BeginUpdate();

            try
            {
                // It should not be necessary to make a copy of files, as if the directory
                // does not exist, the program will already have thrown an exception.
                if (EnvironmentVariable().IsMatch(path))
                {
                    int firstPercent = path.IndexOf('%');
                    int secondPercent = path.IndexOf('%', firstPercent + 1);
                    string variable = path.Substring(firstPercent, secondPercent - firstPercent + 1);
                    if (Environment.GetEnvironmentVariable(variable.Replace("%", "")) == null)
                    {
                        throw new Exception($"The environment variable '{variable}' does not exist");
                    }
                    path = path.Replace(variable, Environment.GetEnvironmentVariable(variable.Replace("%", "")));

                }
                lsvFiles.Items.Clear();
                lsvFiles.Items.AddRange(Directory.GetDirectories(path).Select(d => new ListViewItem(GetFileName(d), 0)).ToArray());
                lsvFiles.Items.AddRange(Directory.GetFiles(path).Select(f => new ListViewItem(GetFileName(f), 1)).ToArray());
                files = lsvFiles.Items.OfType<ListViewItem>()
                    .Where(i => i.ImageIndex != 0 && TagLib.SupportedMimeType
                    .AllExtensions.Contains(i.Text.Substring(i.Text.LastIndexOf('.') + 1)))
                    .Select(i => new FileDisplay(TagLib.File.Create(path + i.Text), i.Index))
                    .ToList();
                UpdateRepository(path);
            }
            catch (Exception ex)
            {
                lsvFiles.Items.Clear();
                lsvFiles.Items.AddRange(itemsBefore);
                MessageBox.Show(ex.Message, "Directory access", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboPath.Text = repository;
            }

            cboPath.EndUpdate();

            UpdateTagList(lsvFiles.SelectedIndices);
        }

        private void UpdateRepository(string path)
        {
            repository = path;
            prgModificationApply.Maximum = files.Count;
        }

        private void UpdateTagList(ListView.SelectedIndexCollection selectedIndices)
        {
            ExtractTagName();
            DisplayMetadataContext();
            lsvMetadataValues.Items.Clear();

            if (cboMetadataList.SelectedIndex == -1)
            {
                btnMetadataChange.Enabled = false;
                return;
            }

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
            object[] hitFiles = FileTags(aimedFiles);
            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < hitFiles.Length; i++)
            {
                items.Add(MetaData(hitFiles[i], i));
            }
            lsvMetadataValues.Items.AddRange(items.ToArray());
            btnMetadataChange.Enabled = aimedFiles.Count != 0;
        }

        private static ListViewItem MetaData(object file, int itterator)
        {
            if (file is TrackDisplay)
            {
                ListViewItem item = new ListViewItem((itterator + 1).ToString());
                item.SubItems.Add((file as TrackDisplay)!.Display);
                item.SubItems.Add(file.ToString());
                return item;
            }
            return new ListViewItem(file.ToString());
        }

        private void ExtractTagName()
        {
            try
            {
                if (cboMetadataList.SelectedItem == null)
                {
                    tagName = (TagName)(-1);
                    return;
                }
                tagName = Enum.Parse<TagName>((cboMetadataList.SelectedItem as TagDisplay)!.Value);
            }
            catch (Exception)
            {
                tagName = (TagName)(-1);
            }
        }

        private object[] FileTags(List<TagLib.File> files)
        {
            switch (tagName)
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
                    return files
                        .Select(f => new TrackDisplay(f))
                        .OrderBy(f => f)
                        .ToArray();
                //case TagName.TrackCount:
                //    return files.Where(f => f.Tag.TrackCount != 0)
                //        .Select(f => f.Tag.TrackCount.ToString())
                //        .Distinct()
                //        .ToArray();
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
            switch (tagName)
            {
                case TagName.Album:
                    file.Tag.Album = txtApplyValue.Text;
                    break;
                case TagName.AlbumArtists:
                    file.Tag.AlbumArtists = txtApplyValue.Text.Split(';');
                    break;
                case TagName.AmazonID:
                    file.Tag.AmazonId = txtApplyValue.Text;
                    break;
                case TagName.Artists:
                    //file.Tag.Artists = txtApplyValue.Text.Split(';');
                    break;
                case TagName.BeatsPerMinute:
                    file.Tag.BeatsPerMinute = (uint)numValues.Value;
                    break;
                case TagName.Comment:
                    file.Tag.Comment = txtApplyValue.Text;
                    break;
                case TagName.Composers:
                    file.Tag.Composers = txtApplyValue.Text.Split(';');
                    break;
                case TagName.ComposersSort:
                    file.Tag.ComposersSort = txtApplyValue.Text.Split(';');
                    break;
                case TagName.Conductor:
                    file.Tag.Conductor = txtApplyValue.Text;
                    break;
                case TagName.Copyright:
                    file.Tag.Copyright = txtApplyValue.Text;
                    break;
                case TagName.Description:
                    file.Tag.Description = txtApplyValue.Text;
                    break;
                case TagName.Disc:
                    file.Tag.Disc = (uint)numValues.Value;
                    break;
                case TagName.DiscCount:
                    file.Tag.DiscCount = (uint)numValues.Value;
                    break;
                case TagName.Genre:
                    file.Tag.Genres = txtApplyValue.Text.Split(';');
                    break;
                case TagName.Grouping:
                    file.Tag.Grouping = txtApplyValue.Text;
                    break;
                case TagName.InitialKey:
                    file.Tag.InitialKey = txtApplyValue.Text;
                    break;
                case TagName.ISRC:
                    file.Tag.ISRC = txtApplyValue.Text;
                    break;
                case TagName.Lyrics:
                    file.Tag.Lyrics = txtApplyValue.Text;
                    break;
                case TagName.Performers:
                    file.Tag.Performers = txtApplyValue.Text.Split(';');
                    break;
                case TagName.PerformersSort:
                    file.Tag.PerformersSort = txtApplyValue.Text.Split(';');
                    break;
                case TagName.PerformersRole:
                    file.Tag.PerformersRole = txtApplyValue.Text.Split(';');
                    break;
                //case TagName.Pictures:
                //    // TODO : Set a specific case for pictures
                //    break;
                case TagName.Publisher:
                    file.Tag.Publisher = txtApplyValue.Text;
                    break;
                case TagName.RemixedBy:
                    file.Tag.RemixedBy = txtApplyValue.Text;
                    break;
                case TagName.Subtitle:
                    file.Tag.Subtitle = txtApplyValue.Text;
                    break;
                case TagName.Title:
                    file.Tag.Title = txtApplyValue.Text;
                    break;
                case TagName.TitleSort:
                    file.Tag.TitleSort = txtApplyValue.Text;
                    break;
                case TagName.Track:
                    TrackDisplay display = new TrackDisplay(file);
                    file.Tag.Track = uint.Parse(lsvMetadataValues.Items.OfType<ListViewItem>()
                        .First(i => i.SubItems[2].Text == display.ToString()).Text);
                    file.Tag.TrackCount = (uint)lsvMetadataValues.Items.Count;
                    break;
                case TagName.Year:
                    file.Tag.Year = (uint)numValues.Value;
                    break;
                default:
                    break;
            }
        }

        private bool ValidInput()
        {
            switch (tagName)
            {
                case TagName.Album:
                case TagName.AlbumArtists:
                case TagName.AmazonID:
                case TagName.Artists:
                    return true;
                case TagName.BeatsPerMinute:
                    return numValues.Value >= 0 && numValues.Value <= 500;
                case TagName.Comment:
                case TagName.Composers:
                case TagName.ComposersSort:
                case TagName.Conductor:
                case TagName.Copyright:
                case TagName.Description:
                    return true;
                case TagName.Disc:
                case TagName.DiscCount:
                    return numValues.Value >= 0 && numValues.Value <= numValues.Maximum;
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
                    return true;
                //case TagName.TrackCount:
                //    return numValues.Value >= 0 && numValues.Value <= numValues.Maximum;
                case TagName.Year:
                    return numValues.Value >= 0 && numValues.Value <= 9999;
                default:
                    return false;
            }
        }

        private void DisplayMetadataContext()
        {
            switch (tagName)
            {
                case TagName.Album:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = cboMetadataList.Text;
                    txtApplyValue.Visible = true;
                    break;
                case TagName.AlbumArtists:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Artist1;Artist2";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.AmazonID:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Amazon ID";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Artists:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Artist1;Artist2";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.BeatsPerMinute:
                    txtApplyValue.Visible = false;
                    numValues.Minimum = 0;
                    numValues.Maximum = 500;
                    numValues.Value = 120;
                    numValues.Visible = true;
                    break;
                case TagName.Comment:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "This is a sample comment";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Composers:
                case TagName.ComposersSort:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Composer1;Composer2";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Conductor:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "John Smith";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Copyright:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Record label company";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Description:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "This is a sample description";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Disc:
                case TagName.DiscCount:
                    // The maximum will most likely never be attained
                    numValues.Visible = true;
                    numValues.Minimum = 0;
                    numValues.Maximum = 1000;
                    numValues.Value = 1;
                    txtApplyValue.Visible = false;
                    break;
                case TagName.Genre:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Genre1;Genre2";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Grouping:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Group description";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.InitialKey:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "C#m";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.ISRC:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "AB-CD1-23-45678";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Lyrics:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "This is a sample Lyrics";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Performers:
                case TagName.PerformersSort:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Performer1;Performer2";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.PerformersRole:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Piano;Bass";
                    txtApplyValue.Visible = true;
                    break;
                //case TagName.Pictures:
                //    break;
                case TagName.Publisher:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Publisher";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.RemixedBy:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Remixer";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Subtitle:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "This is a Subtitle";
                    txtApplyValue.Visible = true;
                    break;
                case TagName.Title:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = cboMetadataList.Text;
                    txtApplyValue.Visible = true;
                    break;
                case TagName.TitleSort:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Title";
                    txtApplyValue.Visible = true;
                    break;
                //case TagName.Track:
                //case TagName.TrackCount:
                //    numValues.Visible = true;
                //    numValues.Minimum = 0;
                //    numValues.Maximum = 1000;
                //    numValues.Value = 1;
                //    txtApplyValue.Visible = false;
                //    break;
                case TagName.Year:
                    numValues.Visible = true;
                    numValues.Minimum = 0;
                    numValues.Maximum = 9999;
                    numValues.Value = 2000;
                    txtApplyValue.Visible = false;
                    break;
                default:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "";
                    txtApplyValue.Visible = true;
                    break;
            }
            DisplayListViewDatas();
        }

        private void DisplayListViewDatas()
        {
            switch (tagName)
            {
                case TagName.Album:
                case TagName.AlbumArtists:
                case TagName.AmazonID:
                case TagName.Artists:
                case TagName.BeatsPerMinute:
                case TagName.Comment:
                case TagName.Composers:
                case TagName.ComposersSort:
                case TagName.Conductor:
                case TagName.Copyright:
                case TagName.Description:
                case TagName.Disc:
                case TagName.DiscCount:
                case TagName.Genre:
                case TagName.Grouping:
                case TagName.InitialKey:
                case TagName.ISRC:
                case TagName.Lyrics:
                case TagName.Performers:
                case TagName.PerformersSort:
                case TagName.PerformersRole:
                case TagName.Publisher:
                case TagName.RemixedBy:
                case TagName.Subtitle:
                case TagName.Title:
                case TagName.TitleSort:
                case TagName.Year:
                default:
                    lsvMetadataValues.Columns.Clear();
                    lsvMetadataValues.Columns.Add("Data", "Data", lsvMetadataValues.Width);
                    //lsvMetadataValues.Columns.Add("Refers", "ReferenceInfo", 0);
                    lsvMetadataValues.HeaderStyle = ColumnHeaderStyle.None;
                    lblSelectMetadata.Text = "Select metadata to apply";
                    lsvMetadataValues.AllowDrop = false;
                    lsvMetadataValues.Cursor = Cursors.Default;
                    grpTrackOrder.Visible = false;
                    break;
                case TagName.Track:
                    lsvMetadataValues.Columns.Clear();
                    lsvMetadataValues.Columns.Add("Num", "No", 50);
                    lsvMetadataValues.Columns.Add("Reference", "Title or File Name", lsvMetadataValues.Width - 50);
                    lsvMetadataValues.Columns.Add("Comparison", "Comparison", 0);
                    lsvMetadataValues.HeaderStyle = ColumnHeaderStyle.Nonclickable;
                    lsvMetadataValues.AllowDrop = true;
                    lsvMetadataValues.Cursor = listCursor;
                    grpTrackOrder.Visible = true;
                    break;
                    //case TagName.TrackCount:
                    //    break;
            }
        }

        /// <summary>
        /// Gets the file or directory name from a full path
        /// </summary>
        /// <param name="path">The full path of the file or directory</param>
        public string GetFileName(string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1);
        }

        [GeneratedRegex("%.*%")]
        private static partial Regex EnvironmentVariable();
    }
}
