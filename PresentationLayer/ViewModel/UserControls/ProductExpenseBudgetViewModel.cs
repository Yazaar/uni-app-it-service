using BusinessLayer;
using Models;
using PresentationLayer.Internals.ObservableModels;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class ProductExpenseBudgetViewModel : BaseViewModel
    {
        public ProductController productController = new ProductController();
        public DepartmentController departmentController = new DepartmentController();

        public IEnumerable<Avdelning> Departments { get; }

        private List<Produktfördelning> productDistributions = new List<Produktfördelning>();
        public List<Produktfördelning> ProductDistributions { get => productDistributions; set { productDistributions = value; OnPropertyChanged(); } }

        private IEnumerable<ObservableAvdelningfördelning> productExpenses = new List<ObservableAvdelningfördelning>();
        public IEnumerable<ObservableAvdelningfördelning> ProductExpenses { get => productExpenses; set { productExpenses = value; OnPropertyChanged(); } }

        private ObservableAvdelningfördelning? selectedProductExpense;
        public ObservableAvdelningfördelning? SelectedProductExpense
        {
            get => selectedProductExpense;
            set { selectedProductExpense = value; OnPropertyChanged(); UpdateDistributions(); }
        }

        private Avdelning? department;
        public Avdelning? Department { get => department; set { department = value; OnPropertyChanged(); UpdateActivityExpenses(); } }
        public string DepartmentVisibility { get => Departments.Count() == 1 ? "Collapsed" : "Visible"; }

        public string LockedText { get => Locked ? "Fastställd" : "Ej fastställd"; }

        private bool locked;
        public bool Locked { get => locked; set { locked = value; OnPropertyChanged(); OnPropertyChanged("LockedText"); } }

        public ICommand BackCommand { get; }
        public ICommand SaveDistributionsCommand { get; }
        public ICommand LockCommand { get; }

        public ProductExpenseBudgetViewModel(MainViewModel mainViewModel)
        {
            Departments = productController.GetAllProductDepartments();

            BackCommand = new RelayCommand(mainViewModel.Back);
            SaveDistributionsCommand = new RelayCommand(SaveDistributions);
            LockCommand = new RelayCommand(Lock);

            switch (mainViewModel.CurrentRole?.RollBehörighet.RollBehörighetID)
            {
                case "driftchef":
                    Departments = Departments.Where(a => a.AvdelningID == "DR");
                    break;
                case "utvchef":
                    Departments = Departments.Where(a => a.AvdelningID == "UF");
                    break;
            }

            if (Departments.Count() == 1)
            {
                Department = Departments.First();
                UpdateActivityExpenses();
            }
        }

        public void Lock()
        {
            if (Department is null) return;
            if (Locked)
            {
                MessageBox.Show("Avdelningen är redan låst");
                return;
            }
            productController.LockDepartmentProductExpenses(Department);
            Department.KostnadsbudgetProduktLåst = true;
            Locked = true;
            MessageBox.Show($"Avdelningen {Department.Benämning} är nu låst");
        }

        public void SaveDistributions()
        {
            if (Locked)
            {
                MessageBox.Show("Kostnadsbudgeten för avdelningen är låst. Går ej att ändra fördelningar");
                return;
            }

            if (SelectedProductExpense is null)
            {
                MessageBox.Show("Välj en kostnadsbudget att redigera");
                return;
            }

            IEnumerable<Produktfördelning> selectedDists = ProductDistributions.Where(pd => pd.Andel > 0);

            if (SelectedProductExpense.CalculateDiff(selectedDists, SelectedProductExpense.Aktivitetfördelningar) < 0)
            {
                MessageBox.Show("Totala fördelningarna på anställd överstiger årsarbetet, var god ändra");
                return;
            }

            Avdelningfördelning newDepartmentDistribution = departmentController.UpdateEmployeeDepartmentProductDistributions(
                SelectedProductExpense.DepartmentDistribution.Personal,
                SelectedProductExpense.DepartmentDistribution.Avdelning,
                selectedDists
            );

            SelectedProductExpense.Produktfördelningar = newDepartmentDistribution.Produktfördelningar;
            MessageBox.Show($"Fördelningar sparade för {SelectedProductExpense.PersonalNamn}");
        }

        public double GetCurrentDistribution(Produkt product)
        {
            if (SelectedProductExpense is null) return 0;
            Produktfördelning? foundProductExpense = SelectedProductExpense.Produktfördelningar.FirstOrDefault(pf => pf.Produkt.ProduktID == product.ProduktID);
            if (foundProductExpense is null) return 0;
            return foundProductExpense.Andel;
        }

        public void UpdateDistributions()
        {
            ProductDistributions = ProductDistributions.Select(ad => {
                ad.Andel = GetCurrentDistribution(ad.Produkt);
                return ad;
            }).ToList();
        }

        public void UpdateActivityExpenses()
        {
            if (Department is null)
            {
                ProductDistributions = new List<Produktfördelning>();
                ProductExpenses = new List<ObservableAvdelningfördelning>();
                Locked = false;
                return;
            }

            ProductDistributions = productController.GetAllProductDistributionsByDepartmentEmpty(Department).ToList();
            ProductExpenses = departmentController.GetAllDepartmentDistributionsByDepartment(Department).Select(ae => new ObservableAvdelningfördelning(ae));
            Locked = Department.KostnadsbudgetProduktLåst;
        }
    }
}
