using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresentationLayer.Internals.ObservableModels
{
    public class ObservableAvdelningfördelning : Notifyable
    {
        private Avdelningfördelning departmentDistribution;
        public Avdelningfördelning DepartmentDistribution { get => departmentDistribution; set { departmentDistribution = value; OnPropertyChanged(); } }

        public string PersonalNamn { get => DepartmentDistribution.Personal.Namn; }

        public double PersonalSysselsättningsgrad { get => DepartmentDistribution.Personal.Sysselsättningsgrad; }

        public double PersonalVakansavdrag { get => DepartmentDistribution.Personal.Vakansavdrag; }

        public double Årsarbete { get => DepartmentDistribution.Andel; }

        public ICollection<Produktfördelning> Produktfördelningar
        {
            get => DepartmentDistribution.Produktfördelningar;
            set { DepartmentDistribution.Produktfördelningar = value; OnPropertyChanged(); OnPropertyChanged("Diff"); }
        }

        public ICollection<Aktivitetfördelning> Aktivitetfördelningar
        {
            get => DepartmentDistribution.Aktivitetfördelningar;
            set { DepartmentDistribution.Aktivitetfördelningar = value; OnPropertyChanged(); OnPropertyChanged("Diff"); }
        }

        public double Diff { get => CalculateDiff(Produktfördelningar, Aktivitetfördelningar); }

        public double CalculateDiff(IEnumerable<Produktfördelning> productDist, IEnumerable<Aktivitetfördelning> activityDist)
        {
            return Årsarbete - productDist.Sum(pd => pd.Andel) - activityDist.Sum(ad => ad.Andel);
        }

        public ObservableAvdelningfördelning(Avdelningfördelning departmentDistribution)
        {
            this.departmentDistribution = departmentDistribution;
        }
    }
}
