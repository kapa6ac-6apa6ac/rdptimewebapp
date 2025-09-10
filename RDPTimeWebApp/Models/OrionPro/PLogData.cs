using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Models.OrionPro
{
    public partial class PLogData
    {
        public DateTime TimeVal { get; set; }
        public int? HozOrgan { get; set; }
        public string Remark { get; set; }
        public int? DoorIndex { get; set; }
        public int? Mode { get; set; }
        public Guid Guid { get; set; }
        public int? Event { get; set; }
    }
}
