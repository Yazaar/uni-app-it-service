using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Konto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int KontoID { get; set; }

        public string Benämning { get; set; }

        public ICollection<SchablonkostnadKontor> SchablonkostnadAvdelningar { get; set; }
        public ICollection<SchablonkostnadProdukt> SchablonkostnadProdukter { get; set; }
        public ICollection<SchablonkostnadAktivitet> SchablonkostnadAktiviteter { get; set; }
    }
}
