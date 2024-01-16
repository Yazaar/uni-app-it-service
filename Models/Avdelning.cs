using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Avdelning
    {
        public string AvdelningID { get; set; }

        public string Benämning { get; set; }
        public bool KostnadsbudgetAktivitetLåst { get; set; }
        public bool KostnadsbudgetProduktLåst { get; set; }

        [Required]
        public Avdelningstyp Avdelningstyp { get; set; }

        public ICollection<Aktivitet> Aktiviteter { get; set; }
        public ICollection<Produkt> Produkter { get; set; }

        public override string ToString()
        {
            return Benämning;
        }
    }
}
