using DataLayer;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BusinessLayer
{
    public class EmployeeController
    {
        public Personal CreateEmployee(string socialSecurityNumber, string name, double monthlySalary, double employmentRate, double vacancyDeduction, IEnumerable<Avdelningfördelning> distribution)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Personal employeeDB = unitOfWork.EmployeeRepository.Find(e => e.Personnummer == socialSecurityNumber);
                if (employeeDB is not null) return null;

                List<Avdelningfördelning> distributionsDB = unitOfWork.DepartmentRepository.FindAll().Select(d => new Avdelningfördelning() { Avdelning = d }).ToList();
                for (int i = distributionsDB.Count - 1; i >= 0; i--)
                {
                    Avdelningfördelning selectedDistribution = distribution.FirstOrDefault(d => d.Avdelning.AvdelningID == distributionsDB[i].Avdelning.AvdelningID);
                    if (selectedDistribution is not null) distributionsDB[i].Andel = selectedDistribution.Andel;
                    else distributionsDB.RemoveAt(i);
                }
                employeeDB = new Personal()
                {
                    Personnummer = socialSecurityNumber,
                    Namn = name,
                    Månadslön = monthlySalary,
                    Sysselsättningsgrad = employmentRate,
                    Vakansavdrag = vacancyDeduction,
                    Avdelningfördelningar = distributionsDB
                };

                unitOfWork.EmployeeRepository.Add(employeeDB);
                unitOfWork.Save();

                return employeeDB;
            }
        }

        public void DeleteEmployee(Personal employee)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Personal employeeDB = unitOfWork.EmployeeRepository.Find(e => e.Personnummer == employee.Personnummer, e => e.Avdelningfördelningar);
                if (employeeDB is null) return;

                unitOfWork.EmployeeRepository.Remove(employeeDB);
                unitOfWork.Save();
            }
        }

        public IEnumerable<Personal> GetAllEmployees()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.EmployeeRepository.FindAll(e => e.Avdelningfördelningar, e => e.Avdelningfördelningar.Select(dd => dd.Avdelning));
            }
        }

        public IEnumerable<Avdelningfördelning> GetEmptyDistributions()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.DepartmentRepository.FindAll().Select(d => new Avdelningfördelning() { Avdelning = d });
            }
        }

        public Personal UpdateEmployee(Personal employee, string name, double monthlySalary, double employmentRate, double vacancyDeduction, List<Avdelningfördelning> distribution)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Personal employeeDB = unitOfWork.EmployeeRepository.Find(e => e.Personnummer == employee.Personnummer, e => e.Avdelningfördelningar, e => e.Avdelningfördelningar.Select(df => df.Avdelning));
                if (employeeDB is null) return null;

                List<Avdelningfördelning> distributionsDB = unitOfWork.DepartmentRepository.FindAll().Select(d => new Avdelningfördelning() { Avdelning = d }).ToList();
                for (int i = distributionsDB.Count - 1; i >= 0; i--)
                {
                    Avdelningfördelning selectedDistribution = distribution.FirstOrDefault(d => d.Avdelning.AvdelningID == distributionsDB[i].Avdelning.AvdelningID);
                    if (selectedDistribution is not null) distributionsDB[i].Andel = selectedDistribution.Andel;
                    else distributionsDB.RemoveAt(i);
                }

                employeeDB.Namn = name;
                employeeDB.Månadslön = monthlySalary;
                employeeDB.Sysselsättningsgrad = employmentRate;
                employeeDB.Vakansavdrag = vacancyDeduction;
                employeeDB.Avdelningfördelningar = distributionsDB;
                
                unitOfWork.Save();

                return employeeDB;
            }
        }

        public Låsning GetLock()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
                return unitOfWork.LocksRepository.Find(l => l.LåsningID == "NEWEMP");
        }

        public void LockEmployees()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Låsning employeeLock = unitOfWork.LocksRepository.Find(l => l.LåsningID == "NEWEMP");
                if (employeeLock is null) return;
                employeeLock.Låst = true;
                unitOfWork.Save();
            }
        }
    }
}
