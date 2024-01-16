using BusinessLayer;
using Models;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class NewEmployeeViewModel : BaseViewModel
    {

        protected readonly MainViewModel mainViewModel;

        protected readonly ObservableCollection<Personal> employees;

        protected readonly EmployeeController employeeController = new EmployeeController();

        private readonly Regex isSSN = new Regex(@"^\d{6}\-\d{4}$");

        public List<Avdelningfördelning> EmployeeDistributions { get; }
        
        private string socialSecurityNumber = string.Empty;
        public string SocialSecurityNumber
        {
            get => socialSecurityNumber;
            set
            {
                if (value.Length <= 11) socialSecurityNumber = value;
                OnPropertyChanged();
            }
        }

        private string name = string.Empty;
        public string Name { get => name; set { name = value; OnPropertyChanged(); } }

        private double monthlySalary;
        public double MonthlySalary { get => monthlySalary; set { monthlySalary = value; OnPropertyChanged(); } }

        private double employmentRate;
        public double EmploymentRate { get => employmentRate; set { employmentRate = value; OnPropertyChanged(); OnPropertyChanged("AnnualWork"); } }

        private double vacancyDeduction;
        public double VacancyDeduction { get => vacancyDeduction; set { vacancyDeduction = value; OnPropertyChanged(); OnPropertyChanged("AnnualWork"); } }

        public double AnnualWork { get => EmploymentRate - VacancyDeduction; }

        public ICommand BackCommand { get; }
        public ICommand SaveEmployeeCommand { get; }

        public NewEmployeeViewModel(MainViewModel mainViewModel, ObservableCollection<Personal> employees)
        {
            this.mainViewModel = mainViewModel;
            this.employees = employees;

            EmployeeDistributions = employeeController.GetEmptyDistributions().ToList();

            BackCommand = new RelayCommand(mainViewModel.Back);
            SaveEmployeeCommand = new RelayCommand(SaveActivity);
        }

        public bool ValidFields()
        {
            if (!isSSN.IsMatch(SocialSecurityNumber))
            {
                MessageBox.Show("Specifiera ett giltigt personnummer (ååååmmdd-xxxx)");
                return false;
            }

            if (MonthlySalary <= 0)
            {
                MessageBox.Show("Specifiera en månadslön över 0");
                return false;
            }
            if (EmploymentRate <= 0 || EmploymentRate > 100)
            {
                MessageBox.Show("Specifiera en sysselsättningsgrad över 0, till och med 100");
                return false;
            }
            if (VacancyDeduction < 0 || VacancyDeduction > 100)
            {
                MessageBox.Show("Specifiera ett vakansavdrag från 0 till och med 100");
                return false;
            }
            if (AnnualWork < 0 || AnnualWork > 100)
            {
                MessageBox.Show("Årsarbetet måste vara från 0 till och med 100");
                return false;
            }

            double distributionSum = EmployeeDistributions.Where(e => e.Andel > 0).Sum(d => d.Andel);
            if (distributionSum != AnnualWork)
            {
                MessageBox.Show($"Säkerställ att årsarbete ({AnnualWork}) och totala avdelningfördelningen ({distributionSum}) stämmer överens");
                return false;
            }

            return true;
        }

        virtual public void SaveActivity()
        {
            if (!ValidFields()) return;

            List<Avdelningfördelning> selectedDistribution = EmployeeDistributions.Where(e => e.Andel > 0).ToList();
            double distributionSum = selectedDistribution.Sum(d => d.Andel);

            Personal createdEmployee = employeeController.CreateEmployee(SocialSecurityNumber, Name, MonthlySalary, EmploymentRate, VacancyDeduction, selectedDistribution);

            if (createdEmployee is null) return;

            employees.Add(createdEmployee);
            mainViewModel.Back();
        }
    }
}
