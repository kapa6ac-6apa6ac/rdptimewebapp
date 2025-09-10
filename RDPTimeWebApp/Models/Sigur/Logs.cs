using System;
using System.Collections.Generic;

namespace RDPTimeWebApp.Models.Sigur
{
    public partial class Logs
    {
        public int Id { get; set; }
        public DateTime? LogTime { get; set; }
        public long Framets { get; set; }
        public int? Area { get; set; }
        public byte[] LogData { get; set; }
        public int? EmpHint { get; set; }
        public int? DevHint { get; set; }
    }
}
