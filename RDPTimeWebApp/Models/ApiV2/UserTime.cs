using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Models.ApiV2
{
    public class UserTime
    {
        public int TimeRdp { get; set; }
        public long TimeScud { get; set; }
        public long TimeScudA { get; set; }
        public DateTime? ScudFrom { get; set; }
        public DateTime? ScudTo { get; set; }
        public long TimeVector { get; set; }
        public long TimeManic { get; set; }

        public int? Id { get; set; }
        public DateTime? Date { get; set; }
        public string? Login { get; set; }
        public string? Name { get; set; }
    }
}
