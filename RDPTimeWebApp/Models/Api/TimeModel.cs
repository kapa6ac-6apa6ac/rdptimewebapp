using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Models.Api
{
    public class TimeModel
    {
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public List<WorkTime> TimeData { get; set; } = new List<WorkTime>();

        public class WorkTime
        {
            public int Id { get; set; }
            public string User { get; set; }
            public string Name { get; set; }
            public long? TimeRDP { get; set; }
            public long? TimeSCUD { get; set; }
            public long? TimeSCUD_R { get; set; }
            public string SCUDStart { get; set; }
            public string SCUDEnd { get; set; }
            public long? TimeVector { get; set; }
            public long? TimeManic { get; set; }
        }
    }
}
