using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class SchablonkostnadProdukt : ISchablonkostnad
    {
        public int SchablonkostnadProduktID { get; set; }

        public double Belopp { get; set; }

        [Required]
        public Produkt Produkter { get; set; }

        [Required]
        public Konto Konto { get; set; }
    }
}
