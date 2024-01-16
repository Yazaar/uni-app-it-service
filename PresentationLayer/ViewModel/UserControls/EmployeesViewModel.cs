using BusinessLayer;
using Models;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class EmployeesViewModel : BaseViewModel
    {
        private readonly MainViewModel mainViewModel;

        private readonly EmployeeController employeeController = new EmployeeController();

        public ObservableCollection<Personal> Employees { get; }

        private Personal? selectedEmployee;
        public Personal? SelectedEmployee { get => selectedEmployee; set { selectedEmployee = value; OnPropertyChanged(); } }

        private bool locked = false;
        public bool Locked { get => locked; set { locked = value; OnPropertyChanged(); OnPropertyChanged("LockedText"); } }
        public string LockedText { get => Locked ? "Fastställd" : "Ej fastställd"; }

        public ICommand BackCommand { get; }
        public ICommand OpenNewEmployeeCommand { get; }
        public ICommand OpenEditEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand LockCommand { get; }

        public EmployeesViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;

            Employees = new ObservableCollection<Personal>(employeeController.GetAllEmployees());

            BackCommand = new RelayCommand(() => mainViewModel.Back());
            OpenNewEmployeeCommand = new RelayCommand(NewEmployee);
            OpenEditEmployeeCommand = new RelayCommand(EditEmployee);
            DeleteEmployeeCommand = new RelayCommand(DeleteEmployee);
            LockCommand = new RelayCommand(LockEmployees);

            Locked = employeeController.GetLock().Låst;
        }

        public void NewEmployee()
        {
            if (Locked)
            {
                MessageBox.Show("Vyn för anställda är låst, går ej att skapa nya anställda");
                return;
            }
            mainViewModel.OpenView(new NewEmployeeViewModel(mainViewModel, Employees));
        }
        
        public void EditEmployee()
        {
            if (Locked)
            {
                MessageBox.Show("Vyn för anställda är låst, går ej att hantera anställda");
                return;
            }
            if (SelectedEmployee is null)
            {
                MessageBox.Show("Var god välj en anställd att redigera");
                return;
            }

            mainViewModel.OpenView(new EditEmployeeViewModel(mainViewModel, SelectedEmployee, Employees));
        }

        public void DeleteEmployee()
        {
            if (Locked)
            {
                MessageBox.Show("Vyn för anställda är låst, går ej att hantera anställda");
                return;
            }
            if (SelectedEmployee is null)
            {
                MessageBox.Show("Var god välj en anställd att redigera");
                return;
            }

            employeeController.DeleteEmployee(SelectedEmployee);
            Employees.Remove(SelectedEmployee);
            MessageBox.Show("Anställd raderad");
        }

        public void LockEmployees()
        {
            if (Locked)
            {
                MessageBox.Show("Vyn för anställda är redan låst");
                return;
            }

            Locked = true;
            employeeController.LockEmployees();

            MessageBox.Show("Vyn för anställda är nu låst");
        }
    }
}
