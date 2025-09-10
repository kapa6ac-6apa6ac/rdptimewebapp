using System;
using System.Collections.Generic;

namespace RDPTimeWebApp.Models.Sigur
{
    public partial class Personal
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Pos { get; set; }
        public string TabId { get; set; }
        public string Status { get; set; }
        public byte[] Codekey { get; set; }
    }
}
