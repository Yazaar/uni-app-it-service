using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Produktkategori
    {
        public int ProduktkategoriID { get; set; }
        public string Benämning { get; set; }

        public ICollection<Produktgrupp> Produktgrupper { get; set; }

        public override string ToString()
        {
            return Benämning;
        }
    }
}
