using System;
using System.Collections.Generic;
using System.Text;

namespace RDPTimeWebApp.Models.Api
{
    public class LogsModel
    {
        public string User { get; set; }
        public string Name { get; set; }

        public List<RDPLog> RDPLogs { get; set; }
        public List<SCUDLog> SCUDLogs { get; set; }

        public class RDPLog
        {
            public DateTime Time { get; set; }
            public string Computer { get; set; }
            public int Duration { get; set; }
        }

        public class SCUDLog
        {
            public int? Mode { get; set; }
            public DateTime Time { get; set; }
            public int? Door { get; set; }
            public int? Event { get; set; }
            public int? City { get; set; }
        }
    }
}
