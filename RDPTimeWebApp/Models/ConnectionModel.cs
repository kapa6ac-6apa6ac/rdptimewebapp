using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Models
{
    public class ConnectionModel
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public UserModel User { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateTime { get; set; }

        [ForeignKey(nameof(Computer))]
        public int ComputerId { get; set; }

        public ComputerModel Computer { get; set; }

        public int Time { get; set; }

        public string IpAddress { get; set; }
    }
}
