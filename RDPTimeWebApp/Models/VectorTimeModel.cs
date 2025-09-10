using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Models
{
    public class VectorTimeModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public UserModel User { get; set; }

        public ushort Year { get; set; }

        public byte Month { get; set; }

        public int Time { get; set; }
    }
}
