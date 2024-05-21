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
        private readonly uint _hashCode;

        public PictureDisplay(TagLib.File file, IPicture picture) : base(file)
        {
            Image = Image.FromStream(new MemoryStream(picture.Data.Data));
            _hashCode = picture.Data.Checksum;
        }

        public override int CompareTo(MetadataDisplay? other)
        {
            if (other is not PictureDisplay)
            {
                return base.CompareTo(other);
            }
            PictureDisplay otherDisplay = (other as PictureDisplay)!;
            return otherDisplay._hashCode.CompareTo(_hashCode);
        }

        public override int GetHashCode()
        {
            return (int)_hashCode;
        }
    }
}
