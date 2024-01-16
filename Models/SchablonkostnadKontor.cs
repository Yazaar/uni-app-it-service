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
    public class SchablonkostnadKontor : ISchablonkostnad
    {
        public int SchablonkostnadKontorID { get; set; }

        public double Belopp { get; set; }

        [Required]
        public Konto Konto { get; set; }
    }
}
