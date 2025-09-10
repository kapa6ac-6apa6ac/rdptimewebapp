using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Models
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }

        public int? ScudSlvId { get; set; }

        public int? ScudUfaId { get; set; }

        public string Login { get; set; }

        public string Name { get; set; }
    }
}
