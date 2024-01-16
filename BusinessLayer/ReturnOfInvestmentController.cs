using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class ReturnOfInvestmentController
    {
        public Avkastningskrav SetReturnOfInvestment(double amount)
        {
            if (amount < 0) return null;
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Avkastningskrav currentROI;

                Avkastningskrav existingROI = unitOfWork.ROIRepository.Find(roi => true);

                if (existingROI is null)
                {
                    currentROI = new Avkastningskrav() { Belopp = amount };
                    unitOfWork.ROIRepository.Add(currentROI);
                }
                else
                {
                    existingROI.Belopp = amount;
                    currentROI = existingROI;
                }

                unitOfWork.Save();
                return currentROI;
            }
        }

        public Avkastningskrav GetReturnOfInvestment()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
                return unitOfWork.ROIRepository.Find(roi => true);
        }
        
        public Låsning GetLock()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
                return unitOfWork.LocksRepository.Find(l => l.LåsningID == "ROI");
        }

        public void LockROI()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Låsning lockDB = unitOfWork.LocksRepository.Find(l => l.LåsningID == "ROI");
                if (lockDB is null) return;
                lockDB.Låst = true;
                unitOfWork.Save();
            }
        }
    }
}
