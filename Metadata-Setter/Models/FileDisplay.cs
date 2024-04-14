using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata_Setter
{
    public class FileDisplay
    {
        public TagLib.File File { get; set; }

        public int Index { get; set; }

        public FileDisplay(TagLib.File file, int index)
        {
            File = file;
            Index = index;
        }
    }
}
