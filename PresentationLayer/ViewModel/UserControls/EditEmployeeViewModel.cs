using Models;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class EditEmployeeViewModel : NewEmployeeViewModel
    {
        private readonly Personal employee;

        public EditEmployeeViewModel(MainViewModel mainViewModel, Personal employee, ObservableCollection<Personal> employees)
            : base(mainViewModel, employees)
        {
            this.employee = employee;

            SocialSecurityNumber = employee.Personnummer;
            Name = employee.Namn;
            MonthlySalary = employee.Månadslön;
            EmploymentRate = employee.Sysselsättningsgrad;
            VacancyDeduction = employee.Vakansavdrag;

            foreach (Avdelningfördelning ed in EmployeeDistributions)
            {
                Avdelningfördelning? pf = employee.Avdelningfördelningar.FirstOrDefault(pf => pf.Avdelning.AvdelningID == ed.Avdelning.AvdelningID);
                if (pf is not null)
                {
                    ed.Andel = pf.Andel;
                }
            }
        }

        public override void SaveActivity()
        {
            if (!ValidFields()) return;

            List<Avdelningfördelning> selectedDistribution = EmployeeDistributions.Where(e => e.Andel > 0).ToList();

            Personal updatedEmployee = employeeController.UpdateEmployee(employee, Name, MonthlySalary, EmploymentRate, VacancyDeduction, selectedDistribution);

            if (updatedEmployee is null) return;

            int productIndex = employees.IndexOf(employee);
            if (productIndex != -1)
            {
                employees[productIndex] = updatedEmployee;
            }

            mainViewModel.Back();
        }
    }
}
