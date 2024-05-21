using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata_Setter.Models
{
    public class AttributeArray<T> where T : class
    {
        public T[] Array { get; set; }

        public AttributeArray(T[] array)
        {
            Array = array;
        }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Array.Length; i++)
            {
                result += Array[i].ToString();
                if (i < Array.Length - 1)
                {
                    result += ";";
                }
            }
            return result;
        }

        public static bool operator ==(AttributeArray<T> a, AttributeArray<T> b)
        {
            if (a.Array.Length != b.Array.Length)
            {
                return false;
            }
            return a.ToString() == b.ToString();
        }

        public static bool operator !=(AttributeArray<T> a, AttributeArray<T> b)
        {
            if (a.Array.Length != b.Array.Length)
            {
                return true;
            }
            return a.ToString() != b.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj is AttributeArray<T> a)
            {
                return this == a;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
