using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class ResultsController
    {
        public IEnumerable<Låsning> GetAllLocks()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                List<Låsning> allLocks = unitOfWork.LocksRepository.FindAll().ToList();

                IEnumerable<Avdelning> departments = unitOfWork.DepartmentRepository.FindAll(d => d.Avdelningstyp);

                foreach (Avdelning d in departments)
                {
                    allLocks.Add(new Låsning()
                    {
                        Benämning = $"Kostnadsbudgetering {d.Benämning}",
                        Låst = d.Avdelningstyp.AvdelningstypID == "AFFO" ? d.KostnadsbudgetAktivitetLåst : d.KostnadsbudgetProduktLåst
                    });
                }

                return allLocks;
            }
        }
    }
}
