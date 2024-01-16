using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Models
{
    public class Prognos
    {
        public int PrognosID { get; set; }
        
        public double Upparbetat { get; set; }
        public double NuPrognos { get; set; }
        
        public string FilProduktID { get; set; }
        public string FilProduktnamn { get; set; }

        [Required]
        public PrognosMånad PrognosMånad { get; set; }

        public DateTime PrognosMånadID { get; set; }

        public ICollection<Prognospost> Prognosposter { get; set; }
        
        [NotMapped]
        public double UtfallAcc { get; set; }

        [NotMapped]
        public Prognos FörgMånProg { get; set; }

        [NotMapped]
        public Produkt Produkt { get; set; }

        [NotMapped]
        public string Produktnamn { get => Produkt is null ? FilProduktnamn : Produkt.Produktnamn; }

        [NotMapped]
        public double Budget
        {
            get
            {
                if (Produkt is null) return 0d;
                return Produkt.Intäktsbudgeteringar.Sum(ib => ib.Budget);
            }
        }

        [NotMapped]
        public double UtfallMån { get => Prognosposter.Sum(pp => pp.Belopp); }

        [NotMapped]
        public double Trend { get => (UtfallAcc + Upparbetat) / PrognosMånadID.Month * 12; }

        [NotMapped]
        public double ProgBudget { get => NuPrognos - Budget; }

        [NotMapped]
        public double FörgProg
        {
            get
            {
                if (FörgMånProg is null) return 0d;
                return FörgMånProg.NuPrognos;
            }
        }

    }
}
