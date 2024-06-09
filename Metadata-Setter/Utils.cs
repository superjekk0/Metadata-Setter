using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace Metadata_Setter
{
    public class Utils
    {
        /// <summary>
        /// Crops an image to a square horizontally centered to comply with
        /// album art standards
        /// </summary>
        /// <param name="picture">Original picture</param>
        public static Bitmap Crop(IPicture picture)
        {
            Bitmap bitmap = new Bitmap(Image.FromStream(new MemoryStream(picture.Data.Data)));
            // The crop must be square and must be centered horizontally
            int size = Math.Min(bitmap.Width, bitmap.Height);
            int x = (bitmap.Width - size) / 2;
            Rectangle dimensionRect = new Rectangle(x, 0, size, size);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawImage(bitmap,
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    dimensionRect, GraphicsUnit.Pixel);
            }
            return bitmap;
        }

        /// <summary>
        /// Gets the file or directory name from a full path
        /// </summary>
        /// <param name="path">The full path of the file or directory</param>
        public static string GetFileName(string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1);
        }


    }
}
