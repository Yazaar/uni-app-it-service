using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class RollBehörighet
    {
        public string RollBehörighetID { get; set; }
        public string Benämning { get; set; }
        public ICollection<Roll> Roller { get; set; }

        public override string ToString()
        {
            return Benämning;
        }
    }
}
