using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Produktgrupp
    {
        public string ProduktgruppID { get; set; }
        public string Benämning { get; set; }

        [Required]
        public Produktkategori Produktkategori { get; set; }
        
        public ICollection<Produkt> Produkter { get; set; }

        public override string ToString()
        {
            return Benämning;
        }
    }
}
