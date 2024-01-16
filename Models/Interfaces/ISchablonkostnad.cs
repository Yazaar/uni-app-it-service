using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interfaces
{
    public interface ISchablonkostnad
    {
        public double Belopp { get; set; }

        public Konto Konto { get; set; }
    }
}
