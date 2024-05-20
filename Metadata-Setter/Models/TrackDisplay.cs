using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata_Setter.Models
{
    public class TrackDisplay : MetadataDisplay
    {
        public uint? TrackNumber { get; set; }

        public TrackDisplay(TagLib.File file) : base(file)
        {
            TrackNumber = file.Tag.Track == 0 ? null : file.Tag.Track;
        }

        public override int CompareTo(MetadataDisplay? other)
        {
            if (other is not TrackDisplay || (TrackNumber == null && (other as TrackDisplay)!.TrackNumber == null))
            {
                return base.CompareTo(other);
            }
            TrackDisplay otherTrack = (other as TrackDisplay)!;
            if (TrackNumber == null)
            {
                return 1;
            }
            if (otherTrack.TrackNumber == null)
            {
                return -1;
            }
            return TrackNumber.Value.CompareTo(otherTrack.TrackNumber.Value);
        }
    }
}
