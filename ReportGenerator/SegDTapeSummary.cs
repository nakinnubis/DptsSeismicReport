using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGenerator
{
   public class SegDTapeSummary
    {
        public string Reel { get; set; }
        public string FirstFFID { get; set; }
        public string LastFFID { get; set; }
        public string FFIDCount { get; set; }
        public string NoOfTrace { get; set; }
        public string FileType { get; set; }
        public string FileSize { get; set; }
    }
}
