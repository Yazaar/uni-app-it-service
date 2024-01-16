using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class DepartmentController
    {
        public IEnumerable<Avdelning> GetAllDepartments()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.DepartmentRepository.FindAll(d => d.Avdelningstyp);
            }
        }

        public Avdelningfördelning UpdateEmployeeDepartmentActivityDistributions(Personal employee, Avdelning department, IEnumerable<Aktivitetfördelning> selectedDists)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Avdelningfördelning departmentDistDB = unitOfWork.DepartmentDistributionRepository.Find(dd =>
                    dd.Avdelning.AvdelningID == department.AvdelningID &&
                    dd.Personal.Personnummer == employee.Personnummer,
                    d => d.Avdelning, d => d.Personal, d => d.Aktivitetfördelningar
                );

                IEnumerable<Aktivitet> activities = unitOfWork.ActivityRepository.FindAll(a => selectedDists.Any(sd => sd.Aktivitet.AktivitetID == a.AktivitetID));
                
                departmentDistDB.Aktivitetfördelningar = activities.Select(a =>
                    new Aktivitetfördelning()
                    {
                        Aktivitet = a,
                        Andel = selectedDists.First(sd => a.AktivitetID == sd.Aktivitet.AktivitetID).Andel
                    }
                ).ToList();

                unitOfWork.Save();
                return departmentDistDB;
            }
        }

        public Avdelningfördelning UpdateEmployeeDepartmentProductDistributions(Personal employee, Avdelning department, IEnumerable<Produktfördelning> selectedDists)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Avdelningfördelning departmentDistDB = unitOfWork.DepartmentDistributionRepository.Find(dd =>
                    dd.Avdelning.AvdelningID == department.AvdelningID &&
                    dd.Personal.Personnummer == employee.Personnummer,
                    d => d.Avdelning, d => d.Personal, d => d.Produktfördelningar
                );

                IEnumerable<Produkt> products = unitOfWork.ProductRepository.FindAll(p => selectedDists.Any(sd => sd.Produkt.ProduktID == p.ProduktID));

                departmentDistDB.Produktfördelningar = products.Select(p =>
                    new Produktfördelning()
                    {
                        Produkt = p,
                        Andel = selectedDists.First(sd => p.ProduktID == sd.Produkt.ProduktID).Andel
                    }
                ).ToList();

                unitOfWork.Save();
                return departmentDistDB;
            }
        }

        public IEnumerable<Avdelningfördelning> GetAllDepartmentDistributionsByDepartment(Avdelning department)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.DepartmentDistributionRepository.FindAll(dd => dd.Avdelning.AvdelningID == department.AvdelningID, d => d.Personal, d => d.Avdelning,
                    d => d.Aktivitetfördelningar, d => d.Aktivitetfördelningar.Select(ad => ad.Aktivitet), d => d.Produktfördelningar, d => d.Produktfördelningar.Select(pd => pd.Produkt));
            }
        }
    }
}
