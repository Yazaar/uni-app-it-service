using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class PrognosMånad
    {
        [NotMapped]
        private static Dictionary<int, string> månader = new Dictionary<int, string>()
        {
            [1] = "Januari",
            [2] = "Februari",
            [3] = "Mars",
            [4] = "April",
            [5] = "Maj",
            [6] = "Juni",
            [7] = "Juli",
            [8] = "Augusti",
            [9] = "September",
            [10] = "Oktober",
            [11] = "November",
            [12] = "December"
        };

        public DateTime PrognosMånadID { get; set; }

        public bool Låst { get; set; }
        
        public ICollection<Prognos> Prognoser { get; set; }

        [NotMapped]
        public string MånadText { get => månader[PrognosMånadID.Month]; }

        public override string ToString()
        {
            return $"{MånadText} {PrognosMånadID.Year}";
        }
    }
}
