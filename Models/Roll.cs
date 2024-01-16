using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Roll
    {
        public int RollID { get; set; }
        public string Benämning { get; set; }
        public string Användarnamn { get; set; }
        public string Lösenord { get; set; }

        [Required]
        public RollBehörighet RollBehörighet { get; set; }
    }
}
