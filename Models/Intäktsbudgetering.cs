using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Intäktsbudgetering
    {
        public int IntäktsbudgeteringID { get; set; }

        public double Avtal { get; set; }
        public string GradA { get; set; }
        public double Tillägg { get; set; }
        public string GradT { get; set; }
        public double Budget { get => Avtal + Tillägg; }
        public double Tim { get; set; }
        public string Kommentar { get; set; }

        [Required]
        public Kund Kund { get; set; }

        [Required]
        public Produkt Produkt { get; set; }
    }
}
