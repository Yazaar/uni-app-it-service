using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Personal
    {
        [Key]
        public string Personnummer { get; set; }

        public string Namn { get; set; }
        public double Månadslön { get; set; }
        public double Sysselsättningsgrad { get; set; }
        public double Vakansavdrag { get; set; }
        public double Årsarbete { get => Sysselsättningsgrad - Vakansavdrag; }

        public ICollection<Avdelningfördelning> Avdelningfördelningar { get; set; }
    }
}
