using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Kundkategori
    {
        public int KundkategoriID { get; set; }
        public string Benämning { get; set; }

        public ICollection<Kund> Kunder { get; set; }

        public override string ToString()
        {
            return Benämning;
        }
    }
}
