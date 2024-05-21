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
        public string? Title { get; set; }

        public TrackDisplay(TagLib.File file) : base(file)
        {
            TrackNumber = file.Tag.Track == 0 ? null : file.Tag.Track;
            Title = file.Tag.Title;
        }

        public override int CompareTo(MetadataDisplay? other)
        {
            if (other is not TrackDisplay)
            {
                return base.CompareTo(other);
            }
            TrackDisplay otherTrack = (other as TrackDisplay)!;
            if (TrackNumber == null && otherTrack.TrackNumber == null)
            {
                if (Title == null && otherTrack.Title == null)
                {
                    return base.CompareTo(otherTrack);
                }
                else if (Title == null)
                {
                    return 1;
                }
                else if (otherTrack.Title == null)
                {
                    return -1;
                }
                return base.CompareTo(otherTrack);
            }
            else if (TrackNumber == null)
            {
                return -1;
            }
            else if (otherTrack.TrackNumber == null)
            {
                return 1;
            }

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
