using PresentationLayer.Internals;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class MenuViewModel : BaseViewModel
    {
        public string SignedInRoleDescription { get; }

        public ViewPermission SystemadminPermission { get; }
        public ViewPermission ROIPermission { get; }
        public ViewPermission ResultsPermission { get; }
        public ViewPermission SchablonPermission { get; }
        public ViewPermission LockRevenueBudgetPermission { get; }
        public ViewPermission RevenueBudgetingPermission { get; }
        public ViewPermission ProductExpenseBudgetingPermission { get; }
        public ViewPermission ActivityExpenseBudgetingPermission { get; }
        public ViewPermission EmployeesPermission { get; }
        
        public ICommand OpenCustomerViewCommand { get; }
        public ICommand OpenActivityViewCommand { get; }
        public ICommand OpenEmployeeViewCommand { get; }
        public ICommand OpenProductViewCommand { get; }
        public ICommand OpenActivityExpenseBudgetViewCommand { get; }
        public ICommand OpenLockRevenueBudgetsViewCommand { get; }
        public ICommand OpenProductExpenseBudgetViewCommand { get; }
        public ICommand OpenPrognosisViewCommand { get; }
        public ICommand OpenResultsViewCommand { get; }
        public ICommand OpenReturnOfInvestmentViewCommand { get; }
        public ICommand OpenRevenueBudgetsByCustomerViewCommand { get; }
        public ICommand OpenRevenueBudgetsByProductViewCommand { get; }
        public ICommand OpenSchablonExpenseViewCommand { get; }
        public ICommand OpenSystemadministrationViewCommand { get; }
        public ICommand LogoutCommand { get; }

        public MenuViewModel(MainViewModel mainViewModel)
        {
            SystemadminPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans" });
            ROIPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "ekonchef" });
            ResultsPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "ekonchef", "fsgmarkchef" });
            SchablonPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "ekonchef", "driftchef", "utvchef", "admchef", "fsgmarkchef" });
            LockRevenueBudgetPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "fsgmarkchef" });
            RevenueBudgetingPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "fsgmarkchef", "pers" });
            ProductExpenseBudgetingPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "driftchef", "utvchef" });
            ActivityExpenseBudgetingPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "fsgmarkchef", "admchef" });
            EmployeesPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "perschef" });

            SignedInRoleDescription = mainViewModel.CurrentRole is null ? "" : mainViewModel.CurrentRole.Benämning;

            OpenCustomerViewCommand = new RelayCommand(() => mainViewModel.OpenView(new CustomerViewModel(mainViewModel)));
            OpenActivityViewCommand = new RelayCommand(() => mainViewModel.OpenView(new ActivitiesViewModel(mainViewModel)));
            OpenEmployeeViewCommand = new RelayCommand(() => mainViewModel.OpenView(new EmployeesViewModel(mainViewModel)));
            OpenProductViewCommand = new RelayCommand(() => mainViewModel.OpenView(new ProductViewModel(mainViewModel)));
            OpenActivityExpenseBudgetViewCommand = new RelayCommand(() => mainViewModel.OpenView(new ActivityExpenseBudgetViewModel(mainViewModel)));
            OpenLockRevenueBudgetsViewCommand = new RelayCommand(() => mainViewModel.OpenView(new LockRevenueBudgetsViewModel(mainViewModel)));
            OpenProductExpenseBudgetViewCommand = new RelayCommand(() => mainViewModel.OpenView(new ProductExpenseBudgetViewModel(mainViewModel)));
            OpenPrognosisViewCommand = new RelayCommand(() => mainViewModel.OpenView(new PrognosisViewModel(mainViewModel)));
            OpenResultsViewCommand = new RelayCommand(() => mainViewModel.OpenView(new ResultsViewModel(mainViewModel)));
            OpenReturnOfInvestmentViewCommand = new RelayCommand(() => mainViewModel.OpenView(new ReturnOfInvestmentViewModel(mainViewModel)));
            OpenRevenueBudgetsByCustomerViewCommand = new RelayCommand(() => mainViewModel.OpenView(new RevenueBudgetsByCustomerViewModel(mainViewModel)));
            OpenRevenueBudgetsByProductViewCommand = new RelayCommand(() => mainViewModel.OpenView(new RevenueBudgetsByProductViewModel(mainViewModel)));
            OpenSchablonExpenseViewCommand = new RelayCommand(() => mainViewModel.OpenView(new SchablonExpenseViewModel(mainViewModel)));
            OpenSystemadministrationViewCommand = new RelayCommand(() => mainViewModel.OpenView(new SystemadministrationViewModel(mainViewModel)));

            LogoutCommand = new RelayCommand(mainViewModel.Back);
        }
    }
}
