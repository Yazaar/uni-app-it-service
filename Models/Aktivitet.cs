using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Aktivitet
    {
        public string AktivitetID { get; set; }

        public string Benämning { get; set; }

        [Required]
        public Avdelning Avdelning { get; set; }

        public ICollection<SchablonkostnadAktivitet> Schablonkostnader { get; set; }
        public ICollection<Aktivitetfördelning> Aktivitetsfördelningar { get; set; }

        public override string ToString()
        {
            return Benämning;
        }
    }
}
