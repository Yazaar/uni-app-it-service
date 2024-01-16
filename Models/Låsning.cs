using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Låsning
    {
        public string LåsningID { get; set; }
        public string Benämning { get; set; }
        public bool Låst { get; set; }

        [NotMapped]
        public string LåstText { get => Låst ? "Fastställd" : "Ej fastställd"; }
    }
}
