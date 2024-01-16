using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class PrognosisController
    {
        private readonly string revenueProductDir = $@"{AppDomain.CurrentDomain.BaseDirectory}IntäktProduktKund";
        private readonly string revenueProductHandledDir = $@"{AppDomain.CurrentDomain.BaseDirectory}IntäktProduktKund\Hanterade";

        public PrognosisController()
        {
            Directory.CreateDirectory(revenueProductDir);
            foreach (string revenueProductFile in Directory.GetFiles(revenueProductDir))
            {
                ImportRevenueFile(revenueProductFile);
                FileUtils.MoveFileToDir(revenueProductFile, revenueProductHandledDir);
            }
        }

        public void ImportRevenueFile(string filepath)
        {
            if (!File.Exists(filepath)) return;
            Regex fileRowRegex = new Regex("\t+");
            using (StreamReader fileStream = new StreamReader(filepath, Encoding.UTF8))
            {
                string line;
                while ((line = fileStream.ReadLine()?.Trim()) is not null)
                {
                    string[] lineParts = fileRowRegex.Split(line);
                    if (lineParts.Length != 6) continue;

                    string productID = lineParts[0];
                    string productName = lineParts[1];
                    string customerID = lineParts[2];
                    int year, month, day;
                    double amount;

                    if (!int.TryParse(lineParts[4].Substring(0, 4), out year)) continue;
                    if (!int.TryParse(lineParts[4].Substring(4, 2), out month)) continue;
                    if (!int.TryParse(lineParts[4].Substring(6, 2), out day)) continue;
                    if (!double.TryParse(lineParts[5], out amount)) continue;

                    amount *= -1;

                    DateTime dateTime;

                    try
                    {
                        dateTime = new DateTime(year, month, day);
                    }
                    catch
                    {
                        continue;
                    }

                    RegisterFileRow(productID, productName, customerID, dateTime, amount);
                }
            }
        }

        private void RegisterFileRow(string productID, string productName, string customerID, DateTime dateTime, double amount)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                if (unitOfWork.LocksRepository.Find(l => l.LåsningID == "PROGNOSIS").Låst) return;

                Prognos prognosisDB = unitOfWork.PrognosisRepository.Find(p =>
                    p.FilProduktID == productID &&
                    p.PrognosMånad.PrognosMånadID.Year == dateTime.Year &&
                    p.PrognosMånad.PrognosMånadID.Month == dateTime.Month,
                    p => p.Prognosposter, p => p.PrognosMånad
                );
                
                if (prognosisDB is null)
                {
                    PrognosMånad prognosisMonthDB = unitOfWork.PrognosisMonthRepository.Find(pm =>
                        pm.PrognosMånadID.Year == dateTime.Year &&
                        pm.PrognosMånadID.Month == dateTime.Month
                    );

                    if (prognosisMonthDB is null)
                    {
                        prognosisMonthDB = new PrognosMånad()
                        {
                            PrognosMånadID = new DateTime(dateTime.Year, dateTime.Month, 1)
                        };
                    }

                    prognosisDB = new Prognos()
                    {
                        FilProduktID = productID,
                        FilProduktnamn = productName,
                        PrognosMånad = prognosisMonthDB,
                        Prognosposter = new List<Prognospost>()
                    };

                    unitOfWork.PrognosisRepository.Add(prognosisDB);
                }

                if (prognosisDB.PrognosMånad.Låst) return;

                if (!prognosisDB.Prognosposter.Any(pp => pp.FilKundID == customerID && pp.Datum.Day == dateTime.Day))
                {
                    prognosisDB.Prognosposter.Add(new Prognospost()
                    {
                        FilKundID = customerID,
                        Belopp = amount,
                        Datum = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day)
                    });
                    unitOfWork.Save();
                }
            }
        }

        public IEnumerable<PrognosMånad> GetAllMonths()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.PrognosisMonthRepository.FindAll();
            }
        }

        private void ParsePrognosis(UnitOfWork unitOfWork, Prognos prognosis)
        {
            prognosis.Produkt = unitOfWork.ProductRepository.Find(p => p.ProduktID == prognosis.FilProduktID,
                        p => p.Intäktsbudgeteringar
                    );

            int searchMonth = prognosis.PrognosMånadID.Month - 1;
            int searchYear = prognosis.PrognosMånadID.Year;
            if (searchMonth == 0)
            {
                searchMonth = 12;
                searchYear -= 1;
            }

            prognosis.FörgMånProg = unitOfWork.PrognosisRepository.Find(p => p.FilProduktID == prognosis.FilProduktID && p.PrognosMånadID.Month == searchMonth && p.PrognosMånadID.Year == searchYear);

            prognosis.UtfallAcc = unitOfWork.PrognosisRepository.FindAll(p =>
                p.FilProduktID == prognosis.FilProduktID && p.PrognosMånadID.Month <= prognosis.PrognosMånadID.Month && p.PrognosMånadID.Year == prognosis.PrognosMånadID.Year,
                p => p.Prognosposter
            ).Sum(p => p.UtfallMån);
        }

        public IEnumerable<Prognos> GetPrognosisesByMonth(int yearNum, int monthNum)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                IEnumerable<Prognos> prognosises = unitOfWork.PrognosisRepository.FindAll(p => p.PrognosMånadID.Year == yearNum && p.PrognosMånadID.Month == monthNum,
                    p => p.Prognosposter);

                foreach (Prognos prognosis in prognosises)
                {
                    ParsePrognosis(unitOfWork, prognosis);
                }

                return prognosises;
            }
        }

        public Prognos UpdatePrognosis(Prognos prognosis, double reprocessed, double currentPrognosis)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Prognos prognosisDB = unitOfWork.PrognosisRepository.Find(p => p.PrognosID == prognosis.PrognosID, p => p.Prognosposter);
                if (prognosisDB is null) return null;

                ParsePrognosis(unitOfWork, prognosisDB);

                prognosisDB.Upparbetat = reprocessed;
                prognosisDB.NuPrognos = currentPrognosis;

                unitOfWork.Save();

                return prognosisDB;
            }
        }

        public int LockMonth(PrognosMånad prognosisMonth)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                int lockCount = 0;
                if (prognosisMonth is not null)
                {
                    PrognosMånad prognosisMonthDB = unitOfWork.PrognosisMonthRepository.Find(pm =>
                        pm.PrognosMånadID.Year == prognosisMonth.PrognosMånadID.Year &&
                        pm.PrognosMånadID.Month == prognosisMonth.PrognosMånadID.Month,
                        p => p.Prognoser
                    );

                    if (prognosisMonthDB is null) return 0;

                    prognosisMonthDB.Låst = true;
                    lockCount = 1;
                }

                if (unitOfWork.PrognosisMonthRepository.Find(pm => !pm.Låst) is null)
                {
                    Låsning lockDB = unitOfWork.LocksRepository.Find(l => l.LåsningID == "PROGNOSIS");
                    if (lockDB is not null) lockDB.Låst = true;
                    lockCount = 2;
                }

                if (lockCount != 0) unitOfWork.Save();

                return lockCount;
            }
        }

        public Låsning GetFullLock()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
                return unitOfWork.LocksRepository.Find(l => l.LåsningID == "PROGNOSIS");
        }
    }
}
