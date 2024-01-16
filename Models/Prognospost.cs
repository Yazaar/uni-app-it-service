using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Prognospost
    {
        public int PrognospostID { get; set; }
        public string FilKundID { get; set; }
        public double Belopp { get; set; }
        public DateTime Datum { get; set; }

        [Required]
        Prognos Prognos { get; set; }
    }
}
