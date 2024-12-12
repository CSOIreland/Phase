using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhaseHarvester
{
    internal class Release
    {
        public Release() { }
        public string MtrCode {get; set; }    
        public string MtrTitle { get; set; }
        public string LngIsoCode { get; set; }
        public int RlsVersion { get; set; }
        public int RlsRevision { get; set; }    
        public DateTime RlsLiveDatetimeFrom { get; set; }
    }
}
