using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata_Setter.Models
{
    public class TagDisplay
    {
        public string Description { get; set; }
        public string Value { get; set; }

        public TagDisplay(string description, string value)
        {
            Description = description;
            Value = value;
        }

        public TagDisplay()
        {
            Description = "";
            Value = "";
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
