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

        public MetadataDisplay(TagLib.File file)
        {
            FileName = file.Name;
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
                return FrmFileManipulator.GetFileName(FileName);
            }
        }

        public override string ToString()
        {
            return "F:" + FrmFileManipulator.GetFileName(FileName);
        }
    }
}
