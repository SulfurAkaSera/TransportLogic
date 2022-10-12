using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportLogic.Models
{
    public class TempPeak
    {
        public int Value { get; set; }
        public bool PeakOnWay { get; set; }
        public bool WrongPeak { get; set; }
    }
}
