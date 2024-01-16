using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Produkt
    {
        public string ProduktID { get; set; }

        public string Produktnamn { get; set; }

        [Required]
        public Produktgrupp Produktgrupp { get; set; }

        [Required]
        public Avdelning Avdelning { get; set; }

        public ICollection<Prognos> Prognos { get; set; }
        public ICollection<Intäktsbudgetering> Intäktsbudgeteringar { get; set; }
        public ICollection<SchablonkostnadProdukt> Schablonkostnader { get; set; }
        public ICollection<Produktfördelning> Produktfördelningar { get; set; }

        public override string ToString()
        {
            return Produktnamn;
        }
    }
}
