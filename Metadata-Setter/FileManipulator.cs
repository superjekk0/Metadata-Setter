// Program written by Charles Mandziuk, (c) 2024
//
// Logiciel �crit par Charles Mandziuk, (c) 2024

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
        private IPicture[] newPictures = new IPicture[] { };

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

        // For a weird reason, the event is triggered as many times as there are items 
        // when we reset the selected indices. It is not THAT important, but it might
        // impact performances.
        private void LsvFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvMetadataValues.SelectedIndices.Count == 0)
            {
                prgModificationApply.Maximum = (lsvFiles.SelectedIndices.Count == 0 ? files.Count : lsvFiles.SelectedIndices.Count);
                UpdateTagList(lsvFiles.SelectedIndices);
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

            // Cleansing the album pictures. We don't care if the pictures are not linked with a file
            IEnumerable<IPicture> pictures = newPictures.ToArray();
            foreach (IPicture picture in newPictures)
            {
                picture.Filename = null;
                picture.Description = null;
            }

            foreach (TagLib.File file in aimedFiles)
            {
                EditFile(file);
                ++prgModificationApply.Value;
            }

            aimedFiles.ForEach(f => f.Save());
            newPictures = pictures.ToArray();
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
            if (lsvFiles.SelectedIndices.Count != 0)
            {
                lsvFiles.EnsureVisible(lsvFiles.SelectedIndices[0]);
            }

            if (tagName != TagName.Track)
            {
                lsvFiles.Focus();
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
                //SelectFiles();
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

        private void LsvMetadata_IndexChanged(object sender, EventArgs e)
        {
            SelectFiles();
        }

        private void LsvFiles_ItemActivate(object sender, EventArgs e)
        {
            lsvMetadataValues.SelectedIndices.Clear();
        }

        private void TxtApplyValue_Click(object sender, EventArgs e)
        {
            if (tagName != TagName.Pictures)
            {
                return;
            }

            FrmPictures pictures = new FrmPictures(newPictures);
            if (pictures.ShowDialog() == DialogResult.OK)
            {
                newPictures = pictures.Pictures.ToArray();
            }
            this.ActiveControl = null;
        }
        //
        // Various utility functions
        //
        private void AutoScrollListView(int index)
        {
            targetedItem = lsvMetadataValues.Items[index];
            lsvMetadataValues.Items[index].Selected = true;
            lsvMetadataValues.Items[index].Focused = true;
            lsvMetadataValues.Items[index].EnsureVisible();
            lsvMetadataValues.Focus();
        }

        private void SelectFiles()
        {
            if (lsvMetadataValues.SelectedIndices.Count != 0)
            {
                files.ForEach(f => lsvFiles.Items[f.Index].Selected = PartOfSelectedMetadata(f.File));
            }
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
                cboMetadataList.DroppedDown = false;
                if (keyData == Keys.Enter)
                {
                    //TxtApplyValue.Focus();
                    UpdateTagList(lsvFiles.SelectedIndices);
                }
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
            string[] filesInDirectory;

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
                string[] subDirectories = Directory.GetDirectories(path);
                for (int i = 0; i < subDirectories.Length; i++)
                {
                    lsvFiles.Items.Add(new ListViewItem(Path.GetFileName(subDirectories[i]), 0));
                }
                filesInDirectory = Directory.GetFiles(path).Where(f => SupportedMimeType.AllExtensions.Contains(Path.GetExtension(f).Remove(0,1))).ToArray();
                foreach (string file in filesInDirectory)
                {
                    lsvFiles.Items.Add(Path.GetFileName(file), 1);
                }
                files = filesInDirectory.AsParallel().Select((f, i) => new FileDisplay(TagLib.File.Create(f, ReadStyle.PictureLazy), subDirectories.Length + i)).ToList();
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
            lsvMetadataValues.BeginUpdate();
            TagName previousTag = tagName;
            ExtractTagName();
            if (tagName != previousTag)
            {
                DisplayMetadataContext();
            }
            lsvMetadataValues.Items.Clear();

            if (cboMetadataList.SelectedIndex == -1)
            {
                btnMetadataChange.Enabled = false;
                lsvMetadataValues.EndUpdate();
                return;
            }

            IEnumerable<FileDisplay> aimedFiles;
            if (selectedIndices.Count != 0)
            {
                aimedFiles = files
                    .Where(f => selectedIndices.Contains(f.Index))
                    .ToList();
            }
            else
            {
                aimedFiles = files;
            }

            //LstMetadataValues.Items.AddRange(FileTags(aimedFiles, CboMetadataList.Text));
            object[] hitFiles = FileTags(aimedFiles);
            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < hitFiles.Length; i++)
            {
                items.Add(MetaData(hitFiles[i], i, lsvMetadataValues.LargeImageList));
            }
            lsvMetadataValues.Items.AddRange(items.ToArray());
            btnMetadataChange.Enabled = aimedFiles.Any();
            lsvMetadataValues.EndUpdate();
        }

        private static ListViewItem MetaData(object file, int itterator, ImageList? images)
        {
            if (file is TrackDisplay)
            {
                ListViewItem item = new ListViewItem((itterator + 1).ToString());
                item.SubItems.Add((file as TrackDisplay)!.Display);
                item.SubItems.Add(file.ToString());
                return item;
            }
            else if (file is PictureDisplay picture)
            {
                ListViewItem item = new ListViewItem(picture.Display);
                item.SubItems.Add(picture.HashCode.ToString());
                if (images != null)
                {
                    images.Images.Add(picture.HashCode.ToString(), picture.Image);
                    item.ImageKey = picture.HashCode.ToString();
                }
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

        private object[] FileTags(IEnumerable<FileDisplay> files)
        {
            switch (tagName)
            {
                case TagName.Album:
                    return files.Where(f => f.File.Tag.Album != null)
                        .DistinctBy(f => f.File.Tag.Album)
                        .Select(f => f.File.Tag.Album)
                        .ToArray();
                case TagName.AlbumArtists:
                    return files.Where(f => f.File.Tag.AlbumArtists.Length != 0)
                        .DistinctBy(f =>
                        {
                            int hash = 0;
                            foreach (string artist in f.File.Tag.AlbumArtists)
                            {
                                hash ^= artist.GetHashCode();
                            }
                            return hash;
                        })
                        .Select(f => new AttributeArray<string>(f.File.Tag.AlbumArtists))
                        .ToArray();
                case TagName.AmazonID:
                    return files.Where(f => f.File.Tag.AmazonId != null)
                        .DistinctBy(f => f.File.Tag.AmazonId)
                        .Select(f => f.File.Tag.AmazonId)
                        .ToArray();
                //case TagName.Artists:
                //    break;
                case TagName.BeatsPerMinute:
                    return files.Where(f => f.File.Tag.BeatsPerMinute != 0)
                        .DistinctBy(f => f.File.Tag.BeatsPerMinute)
                        .Select(f => f.File.Tag.BeatsPerMinute.ToString())
                        .ToArray();
                case TagName.Comment:
                    return files.Where(f => f.File.Tag.Comment != null)
                        .DistinctBy(f => f.File.Tag.Comment)
                        .Select(f => f.File.Tag.Comment)
                        .ToArray();
                case TagName.Composers:
                    return files.Where(f => f.File.Tag.Composers.Length != 0)
                        .DistinctBy(f =>
                        {
                            int hash = 0;
                            foreach (string composer in f.File.Tag.Composers)
                            {
                                hash ^= composer.GetHashCode();
                            }
                            return hash;
                        })
                        .Select(f => new AttributeArray<string>(f.File.Tag.Composers))
                        .ToArray();
                case TagName.ComposersSort:
                    return files.Where(f => f.File.Tag.ComposersSort.Length != 0)
                        .DistinctBy(f =>
                        {
                            int hash = 0;
                            foreach (string composer in f.File.Tag.ComposersSort)
                            {
                                hash ^= composer.GetHashCode();
                            }
                            return hash;
                        })
                        .Select(f => new AttributeArray<string>(f.File.Tag.ComposersSort))
                        .ToArray();
                case TagName.Conductor:
                    return files.Where(f => f.File.Tag.Conductor != null)
                        .DistinctBy(f => f.File.Tag.Conductor)
                        .Select(f => f.File.Tag.Conductor)
                        .ToArray();
                case TagName.Copyright:
                    return files.Where(f => f.File.Tag.Copyright != null)
                        .DistinctBy(f => f.File.Tag.Copyright)
                        .Select(f => f.File.Tag.Copyright)
                        .ToArray();
                case TagName.Description:
                    return files.Where(f => f.File.Tag.Description != null)
                        .DistinctBy(f => f.File.Tag.Description)
                        .Select(f => f.File.Tag.Description)
                        .ToArray();
                case TagName.Disc:
                    return files.Where(f => f.File.Tag.Disc != 0)
                        .DistinctBy(f => f.File.Tag.Disc)
                        .Select(f => f.File.Tag.Disc.ToString())
                        .ToArray();
                case TagName.DiscCount:
                    return files.Where(f => f.File.Tag.DiscCount != 0)
                        .DistinctBy(f => f.File.Tag.DiscCount)
                        .Select(f => f.File.Tag.DiscCount.ToString())
                        .ToArray();
                case TagName.Genre:
                    return files.Where(f => f.File.Tag.Genres.Length != 0)
                        .DistinctBy(f =>
                        {
                            int hash = 0;
                            foreach (string genre in f.File.Tag.Genres)
                            {
                                hash ^= genre.GetHashCode();
                            }
                            return hash;
                        })
                        .Select(f => new AttributeArray<string>(f.File.Tag.Genres))
                        .Distinct()
                        .ToArray();
                case TagName.Grouping:
                    return files.Where(f => f.File.Tag.Grouping != null)
                        .DistinctBy(f => f.File.Tag.Grouping)
                        .Select(f => f.File.Tag.Grouping)
                        .ToArray();
                case TagName.InitialKey:
                    return files.Where(f => f.File.Tag.InitialKey != null)
                        .DistinctBy(f => f.File.Tag.InitialKey)
                        .Select(f => f.File.Tag.InitialKey)
                        .ToArray();
                case TagName.ISRC:
                    return files.Where(f => f.File.Tag.ISRC != null)
                        .DistinctBy(f => f.File.Tag.ISRC)
                        .Select(f => f.File.Tag.ISRC)
                        .ToArray();
                case TagName.Lyrics:
                    return files.Where(f => f.File.Tag.Lyrics != null)
                        .DistinctBy(f => f.File.Tag.Lyrics)
                        .Select(f => f.File.Tag.Lyrics)
                        .ToArray();
                case TagName.Performers:
                    return files.Where(f => f.File.Tag.Performers.Length != 0)
                        .DistinctBy(f =>
                        {
                            int hash = 0;
                            foreach (string performer in f.File.Tag.Performers)
                            {
                                hash ^= performer.GetHashCode();
                            }
                            return hash;
                        })
                        .Select(f => new AttributeArray<string>(f.File.Tag.Performers))
                        .ToArray();
                case TagName.PerformersSort:
                    return files.Where(f => f.File.Tag.PerformersSort.Length != 0)
                        .DistinctBy(f =>
                        {
                            int hash = 0;
                            foreach (string performer in f.File.Tag.PerformersSort)
                            {
                                hash ^= performer.GetHashCode();
                            }
                            return hash;
                        })
                        .Select(f => new AttributeArray<string>(f.File.Tag.PerformersSort))
                        .ToArray();
                case TagName.PerformersRole:
                    return files.Where(f => f.File.Tag.PerformersRole.Length != 0)
                        .DistinctBy(f =>
                        {
                            int hash = 0;
                            foreach (string role in f.File.Tag.PerformersRole)
                            {
                                hash ^= role.GetHashCode();
                            }
                            return hash;
                        })
                        .Select(f => new AttributeArray<string>(f.File.Tag.PerformersRole))
                        .ToArray();
                case TagName.Pictures:
                    return files.Where(f => f.File.Tag.Pictures.Length != 0)
                        .DistinctBy(f =>
                        {
                            uint hash = 0;
                            foreach (IPicture picture in f.File.Tag.Pictures)
                            {
                                hash ^= picture.Data.Checksum;
                            }
                            return hash;
                        })
                        .SelectMany(f => f.File.Tag.Pictures)
                        .DistinctBy(p => p.Data.Checksum)
                        .Select(p => new PictureDisplay(files.First(f => f.File.Tag.Pictures.Contains(p)).File, p))
                        .ToArray();
                case TagName.Publisher:
                    return files.Where(f => f.File.Tag.Publisher != null)
                        .DistinctBy(f => f.File.Tag.Publisher)
                        .Select(f => f.File.Tag.Publisher)
                        .ToArray();
                case TagName.RemixedBy:
                    return files.Where(f => f.File.Tag.RemixedBy != null)
                        .DistinctBy(f => f.File.Tag.RemixedBy)
                        .Select(f => f.File.Tag.RemixedBy)
                        .ToArray();
                case TagName.Subtitle:
                    return files.Where(f => f.File.Tag.Subtitle != null)
                        .DistinctBy(f => f.File.Tag.Subtitle)
                        .Select(f => f.File.Tag.Subtitle)
                        .ToArray();
                case TagName.Title:
                    return files.Where(f => f.File.Tag.Title != null)
                        .DistinctBy(f => f.File.Tag.Title)
                        .Select(f => f.File.Tag.Title)
                        .ToArray();
                case TagName.TitleSort:
                    return files.Where(f => f.File.Tag.TitleSort != null)
                        .DistinctBy(f => f.File.Tag.TitleSort)
                        .Select(f => f.File.Tag.TitleSort)
                        .ToArray();
                case TagName.Track:
                    return files
                        .Select(f => new TrackDisplay(f.File))
                        .Order()
                        .ToArray();
                //case TagName.TrackCount:
                //    return files.Where(f => f.File.Tag.TrackCount != 0)
                //        .Select(f => f.File.Tag.TrackCount.ToString())
                //        .Distinct()
                //        .ToArray();
                case TagName.Year:
                    return files.Where(f => f.File.Tag.Year != 0)
                        .DistinctBy(f => f.File.Tag.Year)
                        .Select(f => f.File.Tag.Year.ToString())
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
                //case TagName.Artists:
                //    //file.Tag.Artists = txtApplyValue.Text.Split(';');
                //    break;
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
                case TagName.Pictures:
                    file.Tag.Pictures = newPictures.ToArray();
                    break;
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
                    //case TagName.Artists:
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
                case TagName.Pictures:
                case TagName.Publisher:
                case TagName.RemixedBy:
                case TagName.Subtitle:
                case TagName.Title:
                case TagName.TitleSort:
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
                //case TagName.Artists:
                //    numValues.Visible = false;
                //    txtApplyValue.PlaceholderText = "Artist1;Artist2";
                //    txtApplyValue.Visible = true;
                //    break;
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
                case TagName.Pictures:
                    numValues.Visible = false;
                    txtApplyValue.PlaceholderText = "Click here to select pictures as attribute";
                    txtApplyValue.Visible = true;
                    break;
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
                //case TagName.Artists:
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
                    lsvMetadataValues.View = View.Details;
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
                    lsvMetadataValues.View = View.Details;
                    lsvMetadataValues.Columns.Clear();
                    lsvMetadataValues.Columns.Add("Num", "No", 50);
                    lsvMetadataValues.Columns.Add("Reference", "Title or File Name", lsvMetadataValues.Width - 50);
                    lsvMetadataValues.Columns.Add("Comparison", "Comparison", 0);
                    lsvMetadataValues.HeaderStyle = ColumnHeaderStyle.Nonclickable;
                    lsvMetadataValues.AllowDrop = true;
                    lsvMetadataValues.Cursor = listCursor;
                    grpTrackOrder.Visible = true;
                    break;
                case TagName.Pictures:
                    lsvMetadataValues.View = View.LargeIcon;
                    lsvMetadataValues.LargeImageList = new ImageList();
                    lsvMetadataValues.LargeImageList.ImageSize = new Size(50, 50);
                    lsvMetadataValues.Columns.Clear();
                    lsvMetadataValues.HeaderStyle = ColumnHeaderStyle.None;
                    lsvMetadataValues.Cursor = Cursors.Default;
                    grpTrackOrder.Visible = false;
                    break;
            }
        }

        private bool PartOfSelectedMetadata(TagLib.File file)
        {
            ListViewItem? item = (tagName != TagName.Track ? lsvMetadataValues.SelectedItems[0] : targetedItem);
            if (item == null)
            {
                return false;
            }

            switch (tagName)
            {
                case TagName.Album:
                    return file.Tag.Album == item.Text;
                case TagName.AlbumArtists:
                    return new AttributeArray<string>(file.Tag.AlbumArtists).ToString() == item.Text;
                case TagName.AmazonID:
                    return file.Tag.AmazonId == item.Text;
                //case TagName.Artists:
                //    break;
                case TagName.BeatsPerMinute:
                    return file.Tag.BeatsPerMinute.ToString() == item.Text;
                case TagName.Comment:
                    return file.Tag.Comment == item.Text;
                case TagName.Composers:
                    return new AttributeArray<string>(file.Tag.Composers).ToString() == item.Text;
                case TagName.ComposersSort:
                    return new AttributeArray<string>(file.Tag.ComposersSort).ToString() == item.Text;
                case TagName.Conductor:
                    return file.Tag.Conductor == item.Text;
                case TagName.Copyright:
                    return file.Tag.Copyright == item.Text;
                case TagName.Description:
                    return file.Tag.Description == item.Text;
                case TagName.Disc:
                    return file.Tag.Disc.ToString() == item.Text;
                case TagName.DiscCount:
                    return file.Tag.DiscCount.ToString() == item.Text;
                case TagName.Genre:
                    return new AttributeArray<string>(file.Tag.Genres).ToString() == item.Text;
                case TagName.Grouping:
                    return file.Tag.Grouping == item.Text;
                case TagName.InitialKey:
                    return file.Tag.InitialKey == item.Text;
                case TagName.ISRC:
                    return file.Tag.ISRC == item.Text;
                case TagName.Lyrics:
                    return file.Tag.Lyrics == item.Text;
                case TagName.Performers:
                    return new AttributeArray<string>(file.Tag.Performers).ToString() == item.Text;
                case TagName.PerformersSort:
                    return new AttributeArray<string>(file.Tag.PerformersSort).ToString() == item.Text;
                case TagName.PerformersRole:
                    return new AttributeArray<string>(file.Tag.PerformersRole).ToString() == item.Text;
                case TagName.Pictures:
                    return file.Tag.Pictures.Any(p => p.Data.Checksum.ToString() == item.SubItems[1].Text);
                // TODO : Make a special implementation for pictures
                case TagName.Publisher:
                    return file.Tag.Publisher == item.Text;
                case TagName.RemixedBy:
                    return file.Tag.RemixedBy == item.Text;
                case TagName.Subtitle:
                    return file.Tag.Subtitle == item.Text;
                case TagName.Title:
                    return file.Tag.Title == item.Text;
                case TagName.TitleSort:
                    return file.Tag.TitleSort == item.Text;
                case TagName.Track:
                    return new TrackDisplay(file).ToString() == item.SubItems[2].Text;
                case TagName.Year:
                    return file.Tag.Year.ToString() == item.Text;
                default:
                    break;
            }
            return false;
        }

        [GeneratedRegex("%.*%")]
        private static partial Regex EnvironmentVariable();
    }
}
