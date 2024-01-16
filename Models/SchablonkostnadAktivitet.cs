using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class SchablonkostnadAktivitet : ISchablonkostnad
    {
        public int SchablonkostnadAktivitetID { get; set; }

        public double Belopp { get; set; }

        [Required]
        public Aktivitet Aktivitet { get; set; }

        [Required]
        public Konto Konto { get; set; }
    }
}
