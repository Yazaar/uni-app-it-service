using BusinessLayer;
using Models;
using PresentationLayer.Internals.ObservableModels;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class ActivityExpenseBudgetViewModel : BaseViewModel
    {
        public ActivityController activityController = new ActivityController();
        public DepartmentController departmentController = new DepartmentController();

        public IEnumerable<Avdelning> Departments { get; }

        private List<Aktivitetfördelning> activityDistributions = new List<Aktivitetfördelning>();
        public List<Aktivitetfördelning> ActivityDistributions { get => activityDistributions; set { activityDistributions = value; OnPropertyChanged(); } }

        private IEnumerable<ObservableAvdelningfördelning> activityExpenses = new List<ObservableAvdelningfördelning>();
        public IEnumerable<ObservableAvdelningfördelning> ActivityExpenses { get => activityExpenses; set { activityExpenses = value; OnPropertyChanged(); } }

        private ObservableAvdelningfördelning? selectedActivityExpense;
        public ObservableAvdelningfördelning? SelectedActivityExpense
        {
            get => selectedActivityExpense;
            set { selectedActivityExpense = value; OnPropertyChanged(); UpdateDistributions(); }
        }

        public string LockedText { get => Locked ? "Fastställd" : "Ej fastställd"; }

        private bool locked;
        public bool Locked { get => locked; set { locked = value; OnPropertyChanged(); OnPropertyChanged("LockedText"); } }

        private Avdelning? department;
        public Avdelning? Department { get => department; set { department = value; OnPropertyChanged(); UpdateActivityExpenses(); } }
        public string DepartmentVisibility { get => Departments.Count() == 1 ? "Collapsed" : "Visible"; }

        public ICommand BackCommand { get; }
        public ICommand SaveDistributionsCommand { get; }
        public ICommand LockCommand { get; }

        public ActivityExpenseBudgetViewModel(MainViewModel mainViewModel)
        {
            Departments = activityController.GetAllActivityDepartments();

            BackCommand = new RelayCommand(mainViewModel.Back);
            SaveDistributionsCommand = new RelayCommand(SaveDistributions);
            LockCommand = new RelayCommand(Lock);

            switch (mainViewModel.CurrentRole?.RollBehörighet.RollBehörighetID)
            {
                case "fsgmarkchef":
                    Departments = Departments.Where(a => a.AvdelningID == "FO");
                    break;
                case "admchef":
                    Departments = Departments.Where(a => a.AvdelningID == "AO");
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

            activityController.LockDepartmentActivityExpenses(Department);
            Department.KostnadsbudgetAktivitetLåst = true;
            Locked = true;
            MessageBox.Show("Avdelningen är nu låst");
        }
        
        public void SaveDistributions()
        {
            if (Locked)
            {
                MessageBox.Show("Kostnadsbudgeten för avdelningen är låst. Går ej att ändra fördelningar");
                return;
            }

            if (SelectedActivityExpense is null)
            {
                MessageBox.Show("Välj en kostnadsbudget att redigera");
                return;
            }

            IEnumerable<Aktivitetfördelning> selectedDists = ActivityDistributions.Where(ad => ad.Andel > 0);

            if (SelectedActivityExpense.CalculateDiff(SelectedActivityExpense.Produktfördelningar, selectedDists) < 0)
            {
                MessageBox.Show("Totala fördelningarna på anställd överstiger årsarbetet, var god ändra");
                return;
            }

            Avdelningfördelning newDepartmentDistribution = departmentController.UpdateEmployeeDepartmentActivityDistributions(
                SelectedActivityExpense.DepartmentDistribution.Personal,
                SelectedActivityExpense.DepartmentDistribution.Avdelning,
                selectedDists
            );

            SelectedActivityExpense.Aktivitetfördelningar = newDepartmentDistribution.Aktivitetfördelningar;
            MessageBox.Show($"Fördelningar sparade för {SelectedActivityExpense.PersonalNamn}");
        }

        public double GetCurrentDistribution(Aktivitet activity)
        {
            if (SelectedActivityExpense is null) return 0;
            Aktivitetfördelning? foundActivityExpense = SelectedActivityExpense.Aktivitetfördelningar.FirstOrDefault(af => af.Aktivitet.AktivitetID == activity.AktivitetID);
            if (foundActivityExpense is null) return 0;
            return foundActivityExpense.Andel;
        }

        public void UpdateDistributions()
        {
            ActivityDistributions = ActivityDistributions.Select(ad => {
                ad.Andel = GetCurrentDistribution(ad.Aktivitet);
                return ad;
            }).ToList();
        }

        public void UpdateActivityExpenses()
        {
            if (Department is null)
            {
                ActivityDistributions = new List<Aktivitetfördelning>();
                ActivityExpenses = new List<ObservableAvdelningfördelning>();
                Locked = false;
                return;
            }

            ActivityDistributions = activityController.GetAllActivityDistributionsByDepartmentEmpty(Department).ToList();
            ActivityExpenses = departmentController.GetAllDepartmentDistributionsByDepartment(Department).Select(ae => new ObservableAvdelningfördelning(ae));
            Locked = Department.KostnadsbudgetAktivitetLåst;
        }
    }
}
