using Microsoft.Data.SqlClient;
using Models;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class UnitOfWork : IDisposable
    {
        // DEBUG CONFIG
        private static bool doReset = false;
        private static bool addExtendedData = false;

        private static bool firstSetup = true;
        private static string resetFile = $@"{AppDomain.CurrentDomain.BaseDirectory}resetdb.txt";

        public Repository<Aktivitet> ActivityRepository { get; private set; }
        public Repository<Aktivitetfördelning> ActivityDistributionRepository { get; private set; }
        public Repository<Avdelningstyp> DepartmentTypeRepository { get; private set; }
        public Repository<Avdelning> DepartmentRepository { get; private set; }
        public Repository<Avkastningskrav> ROIRepository { get; private set; }
        public Repository<Intäktsbudgetering> RevenueBudgetsRepository { get; private set; }
        public Repository<Konto> AccountRepository { get; private set; }
        public Repository<Kund> CustomerRepository { get; private set; }
        public Repository<Kundkategori> CustomerCategoryRepository { get; private set; }
        public Repository<Personal> EmployeeRepository { get; private set; }
        public Repository<Avdelningfördelning> DepartmentDistributionRepository { get; private set; }
        public Repository<Produkt> ProductRepository { get; private set; }
        public Repository<Produktfördelning> ProductDistributionRepository { get; private set; }
        public Repository<Produktgrupp> ProductGroupRepository { get; private set; }
        public Repository<Produktkategori> ProductCategoryRepository { get; private set; }
        public Repository<PrognosMånad> PrognosisMonthRepository { get; private set; }
        public Repository<Prognos> PrognosisRepository { get; private set; }
        public Repository<RollBehörighet> RolePermissionRepository { get; private set; }
        public Repository<Roll> RoleRepository { get; private set; }
        public Repository<SchablonkostnadProdukt> SchablonExpenseProductsRepository { get; private set; }
        public Repository<SchablonkostnadAktivitet> SchablonExpenseActivitiesRepository { get; private set; }
        public Repository<SchablonkostnadKontor> SchablonExpenseOfficeRepository { get; private set; }
        public Repository<Låsning> LocksRepository { get; private set; }

        private readonly AppDbContext appDbContext;
        public UnitOfWork()
        {
            appDbContext = new AppDbContext();

            ActivityRepository = new Repository<Aktivitet>(appDbContext);
            ActivityDistributionRepository = new Repository<Aktivitetfördelning>(appDbContext);
            DepartmentTypeRepository = new Repository<Avdelningstyp>(appDbContext);
            DepartmentRepository = new Repository<Avdelning>(appDbContext);
            ROIRepository = new Repository<Avkastningskrav>(appDbContext);
            RevenueBudgetsRepository = new Repository<Intäktsbudgetering>(appDbContext);
            AccountRepository = new Repository<Konto>(appDbContext);
            CustomerRepository = new Repository<Kund>(appDbContext);
            CustomerCategoryRepository = new Repository<Kundkategori>(appDbContext);
            EmployeeRepository = new Repository<Personal>(appDbContext);
            DepartmentDistributionRepository = new Repository<Avdelningfördelning>(appDbContext);
            ProductRepository = new Repository<Produkt>(appDbContext);
            ProductDistributionRepository = new Repository<Produktfördelning>(appDbContext);
            ProductGroupRepository = new Repository<Produktgrupp>(appDbContext);
            ProductCategoryRepository = new Repository<Produktkategori>(appDbContext);
            PrognosisRepository = new Repository<Prognos>(appDbContext);
            PrognosisMonthRepository = new Repository<PrognosMånad>(appDbContext);
            RolePermissionRepository = new Repository<RollBehörighet>(appDbContext);
            RoleRepository = new Repository<Roll>(appDbContext);
            SchablonExpenseProductsRepository = new Repository<SchablonkostnadProdukt>(appDbContext);
            SchablonExpenseActivitiesRepository = new Repository<SchablonkostnadAktivitet>(appDbContext);
            SchablonExpenseOfficeRepository = new Repository<SchablonkostnadKontor>(appDbContext);
            LocksRepository = new Repository<Låsning>(appDbContext);

            if (firstSetup)
            {
                firstSetup = false;
                if (ShouldExecuteReset())
                {
                    ResetDB();
                    CreateDB();
                    PopulateDB();
                }
            }
        }

        public void Dispose()
        {
            appDbContext.Dispose();
        }
        public void Save()
        {
            appDbContext.SaveChanges();
        }

        private bool ExecuteResetByFile()
        {
            bool reset = false;
            if (File.Exists(resetFile))
            {
                using (StreamReader fileStream = new StreamReader(resetFile))
                {
                    string currentLine = fileStream.ReadLine();
                    if (currentLine is not null && currentLine.Trim() == "1")
                        reset = true;
                }
            }

            using (StreamWriter fileStream = new StreamWriter(resetFile))
                fileStream.Write("0\n\nÄndra 0 till 1 på rad ett för att nollställa HELA databasen vid nästa start");

            return reset;
        }

        private bool ShouldExecuteReset()
        {
            if (doReset) return true;

            if (ExecuteResetByFile()) return true;

            if (InvalidDB()) return true;

            return false;
        }

        private bool InvalidDB()
        {
            try
            {
                Roll role = RoleRepository.Find(r => true);
                if (role is null) return true;
            }
            catch
            {
                return true;
            }
            return false;
        }

        private void ResetDB()
        {
            using (SqlConnection conn = new SqlConnection(appDbContext.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all'; EXEC sp_msforeachtable 'DROP TABLE ?'", conn))
            {
                conn.Open();
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (System.Exception)
                    {
                        // throw;
                    }
                }
                conn.Close();
            }
        }

        private void CreateDB()
        {
            appDbContext.Database.EnsureCreated();
        }

        private void PopulateDB()
        {
            RoleRepository.Add(new Roll()
            {
                Användarnamn = "sys",
                Lösenord = "sys",
                Benämning = "Systemansvarig",
                RollBehörighet = new RollBehörighet() { RollBehörighetID = "systemans", Benämning = "Systemansvarig" }
            });
            RoleRepository.Add(new Roll()
            {
                Användarnamn = "ekonchef",
                Lösenord = "ekonchef",
                Benämning = "Ekonomichef",
                RollBehörighet = new RollBehörighet() { RollBehörighetID = "ekonchef", Benämning = "Ekonomichef" }
            });
            RoleRepository.Add(new Roll()
            {
                Användarnamn = "drift",
                Lösenord = "drift",
                Benämning = "Driftchef",
                RollBehörighet = new RollBehörighet() { RollBehörighetID = "driftchef", Benämning = "Driftchef" }
            });
            RoleRepository.Add(new Roll()
            {
                Användarnamn = "utv",
                Lösenord = "utv",
                Benämning = "Utvecklingschef",
                RollBehörighet = new RollBehörighet() { RollBehörighetID = "utvchef", Benämning = "Utvecklingschef" }
            });
            RoleRepository.Add(new Roll()
            {
                Användarnamn = "fsgmark",
                Lösenord = "fsgmark",
                Benämning = "Försäljsnings- och marknadschef",
                RollBehörighet = new RollBehörighet() { RollBehörighetID = "fsgmarkchef", Benämning = "Försäljsning- och marknadschef" }
            });
            RoleRepository.Add(new Roll()
            {
                Användarnamn = "adm",
                Lösenord = "adm",
                Benämning = "Administrationschef",
                RollBehörighet = new RollBehörighet() { RollBehörighetID = "admchef", Benämning = "Administrationschef" }
            });
            RoleRepository.Add(new Roll()
            {
                Användarnamn = "perschef",
                Lösenord = "perschef",
                Benämning = "Personalchef",
                RollBehörighet = new RollBehörighet() { RollBehörighetID = "perschef", Benämning = "Personalchef" }
            });
            RoleRepository.Add(new Roll()
            {
                Användarnamn = "pers",
                Lösenord = "pers",
                Benämning = "Personal",
                RollBehörighet = new RollBehörighet() { RollBehörighetID = "pers", Benämning = "Personal" }
            });

            Avdelningstyp AFFODepartmentType = new Avdelningstyp() { AvdelningstypID = "AFFO", Benämning = "AFFO" };
            Avdelningstyp ProductionDepartmentType = new Avdelningstyp() { AvdelningstypID = "PROD", Benämning = "Produktion" };

            DepartmentTypeRepository.Add(AFFODepartmentType);
            DepartmentTypeRepository.Add(ProductionDepartmentType);

            Produktkategori productCategory1 = new Produktkategori() { Benämning = "Kundspecifika" };
            Produktkategori productCategory2 = new Produktkategori() { Benämning = "Kundgemensamma" };

            ProductCategoryRepository.Add(productCategory1);
            ProductCategoryRepository.Add(productCategory2);

            Kundkategori customerCategory1 = new Kundkategori()
            {
                Benämning = "Näringsliv"
            };
            Kundkategori customerCategory2 = new Kundkategori()
            {
                Benämning = "Offentlig"
            };

            CustomerCategoryRepository.Add(customerCategory1);
            CustomerCategoryRepository.Add(customerCategory2);
            
            Avdelning departmentDR = new Avdelning()
            {
                AvdelningID = "DR",
                Benämning = "Drift",
                Avdelningstyp = ProductionDepartmentType
            };


            Avdelning departmentAO = new Avdelning()
            {
                AvdelningID = "AO",
                Benämning = "Adm",
                Avdelningstyp = AFFODepartmentType,
            };

            Avdelning departmentUF = new Avdelning()
            {
                AvdelningID = "UF",
                Benämning = "Utv/Förv",
                Avdelningstyp = ProductionDepartmentType
            };
            Avdelning departmentFO = new Avdelning()
            {
                AvdelningID = "FO",
                Benämning = "Förs/Mark",
                Avdelningstyp = AFFODepartmentType
            };

            DepartmentRepository.Add(departmentDR);
            DepartmentRepository.Add(departmentAO);
            DepartmentRepository.Add(departmentUF);
            DepartmentRepository.Add(departmentFO);

            if (addExtendedData)
            {
                Produktgrupp productGroup1 = new Produktgrupp()
                {
                    ProduktgruppID = "G1",
                    Benämning = "Produktgrupp 1",
                    Produktkategori = productCategory1
                };
                Produktgrupp productGroup2 = new Produktgrupp()
                {
                    ProduktgruppID = "G2",
                    Benämning = "Produktgrupp 2",
                    Produktkategori = productCategory2
                };

                ProductGroupRepository.Add(productGroup1);
                ProductGroupRepository.Add(productGroup2);

                Kund customer1 = new Kund()
                {
                    Kundkategori = customerCategory1,
                    KundID = "K111",
                    Kundnamn = "Kund 1"
                };
                Kund customer2 = new Kund()
                {
                    Kundkategori = customerCategory1,
                    KundID = "K222",
                    Kundnamn = "Kund 2"
                };
                Kund customer3 = new Kund()
                {
                    Kundkategori = customerCategory2,
                    KundID = "K333",
                    Kundnamn = "Kund 3"
                };

                CustomerRepository.Add(customer1);
                CustomerRepository.Add(customer2);
                CustomerRepository.Add(customer3);

                departmentDR.Produkter = new List<Produkt>()
                {
                    new Produkt() { Produktgrupp = productGroup1, Produktnamn = "product 1", ProduktID = "PRO1G1" },
                    new Produkt() { Produktgrupp = productGroup2, Produktnamn = "product 2", ProduktID = "PRO2G2" },
                    new Produkt() { Produktgrupp = productGroup1, Produktnamn = "product 3", ProduktID = "PRO3G1" },
                    new Produkt() { Produktgrupp = productGroup1, Produktnamn = "product 4", ProduktID = "PRO4G1" },
                    new Produkt() { Produktgrupp = productGroup2, Produktnamn = "product 5", ProduktID = "PRO5G2" }
                };

                departmentAO.Aktiviteter = new List<Aktivitet>()
                {
                    new Aktivitet() { Benämning = "Aktivitet 1", AktivitetID = "AKT1AO" },
                    new Aktivitet() { Benämning = "Aktivitet 2", AktivitetID = "AKT2AO" },
                    new Aktivitet() { Benämning = "Aktivitet 3", AktivitetID = "AKT3AO" },
                    new Aktivitet() { Benämning = "Aktivitet 4", AktivitetID = "AKT4AO" }
                };

                EmployeeRepository.Add(new Personal()
                {
                    Personnummer = "562567-1437",
                    Månadslön = 30000,
                    Namn = "Personal 1",
                    Sysselsättningsgrad = 100,
                    Avdelningfördelningar = new List<Avdelningfördelning>()
                    {
                        new Avdelningfördelning() { Avdelning = departmentAO, Andel = 50 },
                        new Avdelningfördelning() { Avdelning = departmentDR, Andel = 50 }
                    }
                });
                EmployeeRepository.Add(new Personal()
                {
                    Personnummer = "569527-1437",
                    Månadslön = 30000,
                    Namn = "Personal 2",
                    Sysselsättningsgrad = 100,
                    Avdelningfördelningar = new List<Avdelningfördelning>()
                    {
                        new Avdelningfördelning() { Avdelning = departmentAO, Andel = 100 }
                    }
                });
                EmployeeRepository.Add(new Personal()
                {
                    Personnummer = "197567-1437",
                    Månadslön = 30000,
                    Namn = "Personal 3",
                    Sysselsättningsgrad = 75,
                    Avdelningfördelningar = new List<Avdelningfördelning>()
                    {
                        new Avdelningfördelning() { Avdelning = departmentAO, Andel = 75 }
                    }
                });
            }
            
            LocksRepository.Add(new Låsning()
            {
                LåsningID = "INTBUD",
                Benämning = "Intäktsbudgetering",
                Låst = false
            });
            LocksRepository.Add(new Låsning()
            {
                LåsningID = "SCHOFFICE",
                Benämning = "Schablonkostnad kontor",
                Låst = false
            });
            LocksRepository.Add(new Låsning()
            {
                LåsningID = "NEWEMP",
                Benämning = "Hantera personal",
                Låst = false
            });
            LocksRepository.Add(new Låsning()
            {
                LåsningID = "PROGNOSIS",
                Benämning = "Uppföljning och prognostisering",
                Låst = false
            });
            LocksRepository.Add(new Låsning()
            {
                LåsningID = "DIREXPPROD",
                Benämning = "Direkt kostnad produkt",
                Låst = false
            });
            LocksRepository.Add(new Låsning()
            {
                LåsningID = "DIREXPACT",
                Benämning = "Direkt kostnad aktivitet",
                Låst = false
            });
            LocksRepository.Add(new Låsning()
            {
                LåsningID = "ROI",
                Benämning = "Avkastningskrav",
                Låst = false
            });

            Save();
        }
    }
}
