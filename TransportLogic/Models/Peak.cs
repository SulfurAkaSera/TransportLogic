using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportLogic.Models
{
    public class Peak
    {
        public int I { get; set; }
        public int J { get; set; }
        public bool CorrectCell { get; set; }
        public bool Flag { get; set; }
        public char Sign { get; set; }
    }
}
