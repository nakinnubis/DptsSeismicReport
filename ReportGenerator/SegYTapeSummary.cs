using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGenerator
{
    public class SegYTapeSummary
    {
        public string Reel { get; set; }
        public string LogicalReel { get; set; }
        public string FirstFFID { get; set; }
        public string LastFFID { get; set; }
        public string FFIDCount { get; set; }
        public string FFIDIncrement { get; set; }
        public string TraceCount { get; set; }
        public string FileType { get; set; }
        public string FileSize { get; set; }

    }
}
