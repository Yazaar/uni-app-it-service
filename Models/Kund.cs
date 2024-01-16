using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Kund
    {
        public string KundID { get; set; }
        public string Kundnamn { get; set; }

        [Required]
        public Kundkategori Kundkategori { get; set; }

        public ICollection<Intäktsbudgetering> Intäktsbudgeteringar { get; set; }

        public override string ToString()
        {
            return Kundnamn;
        }
    }
}
