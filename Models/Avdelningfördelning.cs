using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Avdelningfördelning
    {
        public int AvdelningfördelningID { get; set; }
        
        public double Andel { get; set; }

        [Required]
        public Personal Personal { get; set; }

        [Required]
        public Avdelning Avdelning { get; set; }

        public ICollection<Produktfördelning> Produktfördelningar { get; set; }
        public ICollection<Aktivitetfördelning> Aktivitetfördelningar { get; set; }
    }
}
