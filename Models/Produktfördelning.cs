using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Produktfördelning
    {
        public int ProduktfördelningID { get; set; }

        public double Andel { get; set; }

        [Required]
        public Avdelningfördelning Avdelningfördelning { get; set; }

        [Required]
        public Produkt Produkt { get; set; }
    }
}
