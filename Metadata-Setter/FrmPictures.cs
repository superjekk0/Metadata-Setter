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

namespace Metadata_Setter
{
    public partial class FrmPictures : Form
    {
        public List<TagLib.IPicture> Pictures { get; set; }
        public FrmPictures()
        {
            InitializeComponent();
            Pictures = new List<TagLib.IPicture>();
            lsvPictures.LargeImageList = new ImageList();
            lsvPictures.LargeImageList.ImageSize = new Size(64, 64);
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
                foreach (string file in dialog.FileNames)
                {
                    TagLib.Picture picture = new TagLib.Picture(file);
                    Pictures.Add(picture);
                    lsvPictures.LargeImageList!.Images.Add(picture.Data.Checksum.ToString(), Utils.Crop(picture));
                    lsvPictures.Items.Add(file, picture.Data.Checksum.ToString());
                }
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
    }
}
