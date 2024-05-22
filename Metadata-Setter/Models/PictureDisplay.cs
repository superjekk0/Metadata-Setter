using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace Metadata_Setter.Models
{
    public class PictureDisplay : MetadataDisplay
    {
        public Image Image { get; }
        public string? Album { get; set; }
        public uint HashCode { get; }

        public PictureDisplay(TagLib.File file, IPicture picture) : base(file)
        {
            Image = new Bitmap(Image.FromStream(new MemoryStream(picture.Data.Data)), 50, 50);
            Album = file.Tag.Album;
            HashCode = picture.Data.Checksum;
        }

        /// <summary>
        /// Will be useful for displaying full resolution images
        /// </summary>
        public PictureDisplay(TagLib.File file, PictureDisplay display) : base(file)
        {
            Image = Image.FromStream(new MemoryStream(file.Tag.Pictures.First(p => p.Data.Checksum == display.HashCode).Data.Data));
            Album = file.Tag.Album;
            HashCode = display.HashCode;
        }

        public override int CompareTo(MetadataDisplay? other)
        {
            if (other is not PictureDisplay)
            {
                return base.CompareTo(other);
            }
            PictureDisplay otherDisplay = (other as PictureDisplay)!;
            return otherDisplay.HashCode.CompareTo(HashCode);
        }

        public override int GetHashCode()
        {
            return (int)HashCode;
        }

        public override bool Equals(object? obj)
        {
            if (obj is PictureDisplay display)
            {
                return display.HashCode == HashCode;
            }
            return base.Equals(obj);
        }

        public override string Display
        {
            get
            {
                if (Album == null)
                {
                    return base.Display;
                }
                return Album;
            }
        }

        public override string ToString()
        {
            if (Album == null)
            {
                return base.ToString();
            }
            return "A:" + Album;
        }
    }
}
