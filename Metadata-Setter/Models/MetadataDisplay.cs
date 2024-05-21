using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata_Setter.Models
{
    public abstract class MetadataDisplay : IComparable<MetadataDisplay>
    {
        public string FileName { get; set; }
        //public string? Title { get; set; }

        public MetadataDisplay(TagLib.File file)
        {
            FileName = file.Name;
            //Title = file.Tag.Title;
        }

        public virtual int CompareTo(MetadataDisplay? other)
        {
            if (other == null)
            {
                return 1;
            }

            return FileName.CompareTo(other.FileName);
        }

        public virtual string Display
        {
            get
            {
                //if (Title == null)
                //{
                    return FrmFileManipulator.GetFileName(FileName);
                //}
                //return Title;
            }
        }

        public override string ToString()
        {
            //if (Title == null)
            //{
            return "F:" + FrmFileManipulator.GetFileName(FileName);
            //}
            //return "T:" + Title;
        }
    }
}
