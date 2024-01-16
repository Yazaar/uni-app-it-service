using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class RevenueBudgetsController
    {
        private readonly string budgetProduktKundFileDir = $@"{AppDomain.CurrentDomain.BaseDirectory}BudgetProduktKund";

        public Intäktsbudgetering CreateRevenueBudget(Kund customer, Produkt product, double agreement, string gradeA, double addon, string gradeT, double hours, string comment)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Kund customerDB = unitOfWork.CustomerRepository.Find(c => c.KundID == customer.KundID);
                if (customerDB is null) return null;
                
                Produkt productDB = unitOfWork.ProductRepository.Find(p => p.ProduktID == product.ProduktID);
                if (productDB is null) return null;

                Intäktsbudgetering revenueBudgetDB = new Intäktsbudgetering()
                {
                    Kund = customerDB,
                    Produkt = productDB,
                    Avtal = agreement,
                    GradA = gradeA,
                    Tillägg = addon,
                    GradT = gradeT,
                    Tim = hours,
                    Kommentar = comment
                };

                unitOfWork.RevenueBudgetsRepository.Add(revenueBudgetDB);
                unitOfWork.Save();
                return revenueBudgetDB;
            }
        }

        public void DeleteRevenueBudget(Intäktsbudgetering revenueBudget)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Intäktsbudgetering revenueBudgetDB = unitOfWork.RevenueBudgetsRepository.Find(rb => rb.IntäktsbudgeteringID == revenueBudget.IntäktsbudgeteringID);
                if (revenueBudget is null) return;

                unitOfWork.RevenueBudgetsRepository.Remove(revenueBudgetDB);
                unitOfWork.Save();
            }
        }

        public IEnumerable<Intäktsbudgetering> GetAllRevenueBudgets()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.RevenueBudgetsRepository.FindAll(rb => rb.Kund, rb => rb.Produkt, rb => rb.Kund, rb => rb.Produkt.Avdelning);
            }
        }

        public IEnumerable<Intäktsbudgetering> GetRevenueBudgetsByCustomer(Kund customer)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.RevenueBudgetsRepository.FindAll(rb => rb.Kund.KundID == customer.KundID, rb => rb.Kund, rb => rb.Produkt);
            }
        }

        public IEnumerable<Intäktsbudgetering> GetRevenueBudgetsByProduct(Produkt product)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.RevenueBudgetsRepository.FindAll(rb => rb.Produkt.ProduktID == product.ProduktID, rb => rb.Kund, rb => rb.Produkt);
            }
        }

        public Intäktsbudgetering UpdateRevenueBudget(Intäktsbudgetering revenueBudget, Kund customer, Produkt product, double agreement, string gradeA, double addon, string gradeT, double hours, string comment)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Kund customerDB = unitOfWork.CustomerRepository.Find(c => c.KundID == customer.KundID);
                if (customerDB is null) return null;

                Produkt productDB = unitOfWork.ProductRepository.Find(p => p.ProduktID == product.ProduktID);
                if (productDB is null) return null;

                Intäktsbudgetering revenueBudgetDB = unitOfWork.RevenueBudgetsRepository.Find(rb => rb.IntäktsbudgeteringID == revenueBudget.IntäktsbudgeteringID);
                if (revenueBudgetDB is null) return null;

                revenueBudgetDB.Kund = customerDB;
                revenueBudgetDB.Produkt = productDB;
                revenueBudgetDB.Avtal = agreement;
                revenueBudgetDB.GradA = gradeA;
                revenueBudgetDB.Tillägg = addon;
                revenueBudgetDB.GradT = gradeT;
                revenueBudgetDB.Tim = hours;
                revenueBudgetDB.Kommentar = comment;

                unitOfWork.Save();
                return revenueBudgetDB;
            }
        }

        public void LockRevenueBudgets()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Låsning currentLock = unitOfWork.LocksRepository.Find(l => l.LåsningID == "INTBUD");
                if (currentLock is null) return;
                currentLock.Låst = true;
                unitOfWork.Save();
            }
        }
        
        public bool RevenueBudgetsIsLocked()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Låsning currentLock = unitOfWork.LocksRepository.Find(l => l.LåsningID == "INTBUD");
                if (currentLock is null) return false;
                return currentLock.Låst;
            }
        }

        public string ExportRevenueBudgetsTXT(IEnumerable<Intäktsbudgetering> revenueBudgets)
        {
            if (!Directory.Exists(budgetProduktKundFileDir)) Directory.CreateDirectory(budgetProduktKundFileDir);
            DateTime currentTimestamp = DateTime.Now;
            string fullPath = $@"{budgetProduktKundFileDir}\BudgetProduktKund_{currentTimestamp.Year}-{currentTimestamp.Month}-{currentTimestamp.Day}-{currentTimestamp.Hour}-{currentTimestamp.Minute}-{currentTimestamp.Second}.txt";

            using (TextWriter writer = new StreamWriter(fullPath))
            {
                writer.Write("Konto;Ansvar;ProduktID;Produkt;KundID;Kund;Belopp");
                foreach (Intäktsbudgetering rb in revenueBudgets)
                {
                    string departmentName = rb.Produkt.Avdelning.Benämning;
                    if (departmentName == "Utv/Förv") departmentName = "UtvFörv";
                    writer.Write($"\n3010;{departmentName};{rb.Produkt.ProduktID};{rb.Produkt.Produktnamn};{rb.Kund.KundID};{rb.Kund.Kundnamn};{rb.Budget * -1}");
                }
            }
            return fullPath;
        }
    }
}
