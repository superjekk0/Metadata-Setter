using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata_Setter
{
    public class TrackDisplay : IComparable<TrackDisplay>
    {

        public uint? TrackNumber { get; set; }
        public string? Title { get; set; }
        public string FileName { get; set; }

        public TrackDisplay(TagLib.File file)
        {
            TrackNumber = file.Tag.Track == 0 ? null : file.Tag.Track;
            Title = file.Tag.Title;
            FileName = file.Name;
        }

        public int CompareTo(TrackDisplay? other)
        {
            if (other == null)
            {
                return 1;
            }
            if (TrackNumber == null && other.TrackNumber == null)
            {
                if (Title == null && other.Title == null)
                {
                    return FileName.CompareTo(other.FileName);
                }
                else if (Title == null)
                {
                    return -1;
                }
                else if (other.Title == null)
                {
                    return 1;
                }
                return FileName.CompareTo(other.FileName);
            }
            if (TrackNumber == null)
            {
                return 1;
            }
            if (other.TrackNumber == null)
            {
                return -1;
            }
            return TrackNumber.Value.CompareTo(other.TrackNumber.Value);
        }

        public override string ToString()
        {
            if (Title == null)
            {
                return FileName.Substring(FileName.LastIndexOf('\\') + 1);
            }
            return Title;
        }
    }
}
