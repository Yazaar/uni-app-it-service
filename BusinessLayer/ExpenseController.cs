using DataLayer;
using Models;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class ExpenseController
    {
        public IEnumerable<SchablonkostnadAktivitet> GetActivitySchablons(Aktivitet activity, int accountPlan)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                IEnumerable<SchablonkostnadAktivitet> activitySchablonsDB = unitOfWork.ActivityRepository.Find(a => a.AktivitetID == activity.AktivitetID, a => a.Schablonkostnader, a => a.Schablonkostnader.Select(s => s.Konto))?.Schablonkostnader;
                if (activitySchablonsDB is null) return null;

                // accountPlan = 5
                int minAccountPlan = accountPlan * 1000; // 5000
                int maxAccountPlan = (accountPlan + 1) * 1000; // 6000

                IEnumerable<Konto> accounts = unitOfWork.AccountRepository.FindAll(a => a.KontoID >= minAccountPlan && a.KontoID < maxAccountPlan);

                SchablonkostnadAktivitet[] schablons = new SchablonkostnadAktivitet[accounts.Count()];
                int schablonIndex = 0;

                foreach (Konto account in accounts)
                {
                    SchablonkostnadAktivitet foundSchablon = activitySchablonsDB.FirstOrDefault(s => s.Konto.KontoID == account.KontoID);
                    if (foundSchablon is null) schablons[schablonIndex++] = new SchablonkostnadAktivitet() { Konto = account };
                    else schablons[schablonIndex++] = foundSchablon;
                }

                return schablons;
            }
        }

        public IEnumerable<SchablonkostnadKontor> GetOfficeSchablons(int accountPlan)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                // accountPlan = 5
                int minAccountPlan = accountPlan * 1000; // 5000
                int maxAccountPlan = (accountPlan + 1) * 1000; // 6000

                IEnumerable<SchablonkostnadKontor> officeSchablonsDB = unitOfWork.SchablonExpenseOfficeRepository.FindAll(s => s.Konto.KontoID >= minAccountPlan && s.Konto.KontoID < maxAccountPlan, s => s.Konto);
                if (officeSchablonsDB is null) return null;

                IEnumerable<Konto> accounts = unitOfWork.AccountRepository.FindAll(a => a.KontoID >= minAccountPlan && a.KontoID < maxAccountPlan);

                SchablonkostnadKontor[] schablons = new SchablonkostnadKontor[accounts.Count()];
                int schablonIndex = 0;

                foreach (Konto account in accounts)
                {
                    SchablonkostnadKontor foundSchablon = officeSchablonsDB.FirstOrDefault(s => s.Konto.KontoID == account.KontoID);
                    if (foundSchablon is null) schablons[schablonIndex++] = new SchablonkostnadKontor() { Konto = account };
                    else schablons[schablonIndex++] = foundSchablon;
                }

                return schablons;
            }
        }

        public IEnumerable<SchablonkostnadProdukt> GetProductSchablons(Produkt product, int accountPlan)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                IEnumerable<SchablonkostnadProdukt> productSchablonsDB = unitOfWork.ProductRepository.Find(p => p.ProduktID == product.ProduktID, p => p.Schablonkostnader, p => p.Schablonkostnader.Select(s => s.Konto))?.Schablonkostnader;
                if (productSchablonsDB is null) return null;

                // accountPlan = 5
                int minAccountPlan = accountPlan * 1000; // 5000
                int maxAccountPlan = (accountPlan + 1) * 1000; // 6000

                IEnumerable<Konto> accounts = unitOfWork.AccountRepository.FindAll(a => a.KontoID >= minAccountPlan && a.KontoID < maxAccountPlan);

                SchablonkostnadProdukt[] schablons = new SchablonkostnadProdukt[accounts.Count()];
                int schablonIndex = 0;

                foreach (Konto account in accounts)
                {
                    SchablonkostnadProdukt foundSchablon = productSchablonsDB.FirstOrDefault(s => s.Konto.KontoID == account.KontoID);
                    if (foundSchablon is null) schablons[schablonIndex++] = new SchablonkostnadProdukt() { Konto = account };
                    else schablons[schablonIndex++] = foundSchablon;
                }

                return schablons;
            }
        }

        public SchablonkostnadProdukt SaveProductSchablonExpense(Produkt product, SchablonkostnadProdukt productSchablon, double amount)
        {
            if (amount < 0) return null;
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Konto accountDB = unitOfWork.AccountRepository.Find(a => a.KontoID == productSchablon.Konto.KontoID);
                if (accountDB is null) return null;

                Produkt productDB = unitOfWork.ProductRepository.Find(p => p.ProduktID == product.ProduktID,
                    p => p.Schablonkostnader, p => p.Schablonkostnader.Select(s => s.Konto)
                );
                if (productDB is null) return null;

                SchablonkostnadProdukt foundSchablon = productDB.Schablonkostnader.FirstOrDefault(s => s.Konto.KontoID == accountDB.KontoID);

                if (foundSchablon is null)
                {
                    foundSchablon = new SchablonkostnadProdukt()
                    {
                        Konto = accountDB,
                        Belopp = amount
                    };

                    productDB.Schablonkostnader.Add(foundSchablon);
                }
                else
                {
                    foundSchablon.Belopp = amount;
                }

                unitOfWork.Save();
                return foundSchablon;
            }
        }

        public SchablonkostnadKontor SaveOfficeSchablonExpense(SchablonkostnadKontor officeSchablon, double amount)
        {
            if (amount < 0) return null;
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Konto accountDB = unitOfWork.AccountRepository.Find(a => a.KontoID == officeSchablon.Konto.KontoID);
                if (accountDB is null) return null;

                SchablonkostnadKontor foundOfficeSchablon = unitOfWork.SchablonExpenseOfficeRepository.Find(s => s.Konto.KontoID == officeSchablon.Konto.KontoID, s => s.Konto);

                if (foundOfficeSchablon is null)
                {
                    foundOfficeSchablon = new SchablonkostnadKontor() { Konto = accountDB, Belopp = amount };
                    unitOfWork.SchablonExpenseOfficeRepository.Add(foundOfficeSchablon);
                }
                else
                {
                    foundOfficeSchablon.Belopp = amount;
                }

                unitOfWork.Save();
                return foundOfficeSchablon;
            }
        }

        public SchablonkostnadAktivitet SaveActivitySchablonExpense(Aktivitet activity, SchablonkostnadAktivitet activitySchablon, double amount)
        {
            if (amount < 0) return null;
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Konto accountDB = unitOfWork.AccountRepository.Find(a => a.KontoID == activitySchablon.Konto.KontoID);
                if (accountDB is null) return null;

                Aktivitet activityDB = unitOfWork.ActivityRepository.Find(a => a.AktivitetID == a.AktivitetID,
                    a => a.Schablonkostnader, a => a.Schablonkostnader.Select(s => s.Konto)
                );
                if (activityDB is null) return null;

                SchablonkostnadAktivitet foundSchablon = activityDB.Schablonkostnader.FirstOrDefault(s => s.Konto.KontoID == accountDB.KontoID);

                if (foundSchablon is null)
                {
                    foundSchablon = new SchablonkostnadAktivitet()
                    {
                        Konto = accountDB,
                        Belopp = amount
                    };

                    activityDB.Schablonkostnader.Add(foundSchablon);
                }
                else
                {
                    foundSchablon.Belopp = amount;
                }

                unitOfWork.Save();
                return foundSchablon;
            }
        }

        public void LockOfficeSchablon()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Låsning schablonOfficeLock = unitOfWork.LocksRepository.Find(l => l.LåsningID == "SCHOFFICE");
                if (schablonOfficeLock is null) return;
                schablonOfficeLock.Låst = true;
                unitOfWork.Save();
            }
        }

        public void LockProductDirectExpense()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Låsning lockDB = unitOfWork.LocksRepository.Find(l => l.LåsningID == "DIREXPPROD");
                if (lockDB is null) return;
                lockDB.Låst = true;
                unitOfWork.Save();
            }
        }

        public void LockActivityDirectExpense()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Låsning lockDB = unitOfWork.LocksRepository.Find(l => l.LåsningID == "DIREXPACT");
                if (lockDB is null) return;
                lockDB.Låst = true;
                unitOfWork.Save();
            }
        }

        public Låsning GetOfficeSchablonLock()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
                return unitOfWork.LocksRepository.Find(l => l.LåsningID == "SCHOFFICE");
        }

        public Låsning GetProductDirectExpenseLock()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
                return unitOfWork.LocksRepository.Find(l => l.LåsningID == "DIREXPPROD");
        }

        public Låsning GetActivityDirectExpenseLock()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
                return unitOfWork.LocksRepository.Find(l => l.LåsningID == "DIREXPACT");
        }
    }
}
