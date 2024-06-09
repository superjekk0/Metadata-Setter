using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Metadata_Setter
{
    public partial class FrmPictures : Form
    {
        public List<IPicture> Pictures { get; set; }
        public FrmPictures(IEnumerable<IPicture> pictures)
        {
            InitializeComponent();
            lsvPictures.LargeImageList = new ImageList();
            lsvPictures.LargeImageList.ImageSize = new Size(64, 64);
            if (pictures.Any())
            {
                Pictures = new List<IPicture>(pictures);
                AppendPictures(Pictures);
            }
            else
            {
                Pictures = new List<IPicture>();
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                AppendPictures(dialog.FileNames);
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            Pictures.Clear();
            lsvPictures.Items.Clear();
            lsvPictures.LargeImageList!.Images.Clear();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            Pictures = Pictures.Where(p => lsvPictures.SelectedItems.OfType<ListViewItem>()
            .Any(i => i.ImageKey == p.Data.Checksum.ToString()))
                .ToList();
            foreach (ListViewItem item in lsvPictures.SelectedItems)
            {
                lsvPictures.LargeImageList!.Images.RemoveByKey(item.ImageKey);
                lsvPictures.Items.Remove(item);
            }
        }

        private void BtnAccept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void AppendPictures(IEnumerable<string> filePaths)
        {
            foreach (string file in filePaths)
            {
                Picture picture = new Picture(file);
                // To avoid duplicates in the list
                if (!Pictures.Any(p => p.Data.Checksum == picture.Data.Checksum))
                {
                    Pictures.Add(picture);
                    lsvPictures.LargeImageList!.Images.Add(picture.Data.Checksum.ToString(), Utils.Crop(picture));
                    lsvPictures.Items.Add(Utils.GetFileName(file), picture.Data.Checksum.ToString());
                }
            }
        }

        private void AppendPictures(IEnumerable<IPicture> pictures)
        {
            IEnumerable<IPicture> distinctPictures = pictures.DistinctBy(p => p.Data.Checksum).ToArray();
            foreach (IPicture picture in distinctPictures)
            {
                Pictures.Add(picture);
                lsvPictures.LargeImageList!.Images.Add(picture.Data.Checksum.ToString(), Utils.Crop(picture));
                lsvPictures.Items.Add(picture.Filename, picture.Data.Checksum.ToString());
            }
        }
    }
}
