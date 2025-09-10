using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Models
{
    public class ComputerModel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
