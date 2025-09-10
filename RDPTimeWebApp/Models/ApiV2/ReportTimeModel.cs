using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Models.ApiV2
{
    public class ReportTimeModel
    {
        public string Date { get; set; }

        public List<WorkTime> TimeData { get; set; } = new List<WorkTime>();

        public class WorkTime
        {
            public string User { get; set; }
            public string Name { get; set; }
            public TimeSpan TimeRDP { get; set; }
            public TimeSpan TimeSCUD { get; set; }
            public TimeSpan TimeSCUD_R { get; set; }
            public TimeSpan? TimeVector { get; set; }
            public TimeSpan? ManicTime { get; set; }
        }
    }
}
