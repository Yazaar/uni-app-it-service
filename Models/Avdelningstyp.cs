using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Avdelningstyp
    {
        public string AvdelningstypID { get; set; }
        public string Benämning { get; set; }

        public ICollection<Avdelning> Avdelningar { get; set; }
    }
}
