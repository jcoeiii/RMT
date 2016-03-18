using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tugwell
{
    public class SortableRow : IComparable<SortableRow>
    {
        public string PO { get; set; }
        public int Row { get; set; }

        public int Compare(SortableRow sr)
        {
            return this.PO.CompareTo(sr.PO);
        }

        public int CompareTo(SortableRow other)
        {
            return this.PO.CompareTo(other.PO);
        }
    }
}