using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Avkastningskrav
    {
        public int AvkastningskravID { get; set; }

        [NotMapped]
        public int Kontonummer { get; } = 9999;

        public double Belopp { get; set; }
    }
}
