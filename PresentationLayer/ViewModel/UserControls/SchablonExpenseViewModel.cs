using BusinessLayer;
using Models;
using Models.Interfaces;
using PresentationLayer.Internals;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class SchablonExpenseViewModel : BaseViewModel
    {
        private readonly ActivityController activityController = new ActivityController();
        private readonly ProductController productController = new ProductController();
        private readonly ExpenseController expenseController = new ExpenseController();

        private readonly NotifyCollectionChangedEventHandler collectionChangedHandler;

        private IEnumerable<string>? accountPlans;
        public IEnumerable<string>? AccountPlans { get => accountPlans; set { accountPlans = value; OnPropertyChanged(); } }

        private IEnumerable<string> accountPlansOffice = new string[] { "Kontoplan 4", "Kontoplan 5"};
        private IEnumerable<string> accountPlansProductActivity = new string[] { "Kontoplan 6", "Kontoplan 7", "Kontoplan 8" };

        private int selectedAccountPlanIndex = -1;
        public int SelectedAccountPlanIndex { get => selectedAccountPlanIndex; set { selectedAccountPlanIndex = value; OnPropertyChanged(); LoadSchablon(); } }

        public IEnumerable<string> Categories { get; } = new string[] { "Kontor", "Aktivitet", "Produkt" };

        private int? selectedCategoryIndex;
        public int? SelectedCategoryIndex { get => selectedCategoryIndex; set { selectedCategoryIndex = value; SelectedSubCategory = null;
                                                                                OnPropertyChanged(); OnPropertyChanged("LockPermission"); LoadSubcategory(); } }

        private readonly ViewPermission defaultPermission = new ViewPermission(null, new string[] { });
        private readonly ViewPermission[] lockPermissions;
        public ViewPermission LockPermission
        {
            get
            {
                if (SelectedCategoryIndex is null || SelectedCategoryIndex > 2 || SelectedCategoryIndex < 0) return defaultPermission;
                return lockPermissions[(int)SelectedCategoryIndex];
            }
        }

        private IEnumerable<object>? subCategories;
        public IEnumerable<object>? SubCategories { get => subCategories; set { subCategories = value; OnPropertyChanged(); } }

        private object? selectedSubCategory;
        public object? SelectedSubCategory { get => selectedSubCategory; set { selectedSubCategory = value; OnPropertyChanged(); LoadSchablon(); } }

        private double amount;
        public double Amount { get => amount; set { amount = value; OnPropertyChanged(); } }

        private ObservableCollection<ISchablonkostnad>? schablonExpenses;
        public ObservableCollection<ISchablonkostnad>? SchablonExpenses
        {
            get => schablonExpenses;
            set
            {
                if (schablonExpenses is not null) schablonExpenses.CollectionChanged -= collectionChangedHandler;
                schablonExpenses = value;
                if (schablonExpenses is not null) schablonExpenses.CollectionChanged += collectionChangedHandler;
                OnPropertyChanged();
                UpdateSums();
            }
        }

        private double amountSum;
        public double AmountSum { get => amountSum; set { amountSum = value; OnPropertyChanged(); } }

        private ISchablonkostnad? selectedSchablon;
        public ISchablonkostnad? SelectedSchablon { get => selectedSchablon; set { selectedSchablon = value; OnPropertyChanged(); LoadAmount(); } }

        private bool locked;
        public bool Locked { get => locked; set { locked = value; OnPropertyChanged(); OnPropertyChanged("LockedText"); } }

        public string LockedText { get => Locked ? "Fastställd" : "Ej fastställd"; }

        public ICommand BackCommand { get; }
        public ICommand SaveAmountCommand { get; }
        public ICommand LockCommand { get; }

        public SchablonExpenseViewModel(MainViewModel mainViewModel)
        {
            collectionChangedHandler = new NotifyCollectionChangedEventHandler((object? sender, NotifyCollectionChangedEventArgs e) => UpdateSums());

            BackCommand = new RelayCommand(mainViewModel.Back);
            SaveAmountCommand = new RelayCommand(SaveAmount);
            LockCommand = new RelayCommand(Lock);

            lockPermissions = new ViewPermission[]
            {
                new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "ekonchef" }),
                new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "ekonchef", "admchef", "fsgmarkchef" }),
                new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "ekonchef", "driftchef", "utvchef" })
            };
        }

        public void LoadAmount()
        {
            Amount = SelectedSchablon is null ? 0 : SelectedSchablon.Belopp;
        }

        public void UpdateSums()
        {
            double newAmountSum = 0;

            if (SchablonExpenses is not null)
            {
                foreach (ISchablonkostnad s in SchablonExpenses)
                {
                    newAmountSum += s.Belopp;
                }
            }

            AmountSum = newAmountSum;
        }

        public void LoadSubcategory()
        {
            switch (SelectedCategoryIndex)
            {
                case 0:
                    SubCategories = null;
                    AccountPlans = accountPlansOffice;
                    break;
                case 1:
                    SubCategories = activityController.GetAllActivities().OrderBy(a => a.Benämning);
                    AccountPlans = accountPlansProductActivity;
                    break;
                case 2:
                    SubCategories = productController.GetAllProducts().OrderBy(p => p.Produktnamn);
                    AccountPlans = accountPlansProductActivity;
                    break;
                default:
                    SubCategories = null;
                    AccountPlans = null;
                    break;
            }
        }

        public void LoadSchablon()
        {   
            if (SelectedCategoryIndex == 0) // Office
            {
                if (SelectedAccountPlanIndex != -1)
                    SchablonExpenses = new ObservableCollection<ISchablonkostnad>(expenseController.GetOfficeSchablons(SelectedAccountPlanIndex + 4));
                else SchablonExpenses = null;
                Locked = expenseController.GetOfficeSchablonLock().Låst;
                return;
            }
            
            if (SelectedSubCategory is null)
            {
                SchablonExpenses = null;
                Locked = false;
                return;
            }

            if (SelectedSubCategory is Produkt product)
            {
                if (SelectedAccountPlanIndex != -1)
                    SchablonExpenses = new ObservableCollection<ISchablonkostnad>(expenseController.GetProductSchablons(product, SelectedAccountPlanIndex + 6));
                else SchablonExpenses = null;
                Locked = expenseController.GetProductDirectExpenseLock().Låst;
            }
            else if (SelectedSubCategory is Aktivitet activity)
            {
                if (SelectedAccountPlanIndex != -1)
                    SchablonExpenses = new ObservableCollection<ISchablonkostnad>(expenseController.GetActivitySchablons(activity, SelectedAccountPlanIndex + 6));
                else SchablonExpenses = null;
                Locked = expenseController.GetActivityDirectExpenseLock().Låst;
            }
        }

        public void SaveAmount()
        {
            if (Locked)
            {
                MessageBox.Show("Kostnaden är redan låst och kan därför inte ändra beloppet");
                return;
            }
            if (SelectedSchablon is null || SchablonExpenses is null)
            {
                MessageBox.Show("Var god välj en kostnad att redigera");
                return;
            }

            if (Amount < 0)
            {
                MessageBox.Show("Kostnaden kan inte vara negativ");
                return;
            }

            ISchablonkostnad newSchablon;

            if (SelectedSchablon is SchablonkostnadProdukt sp && SelectedSubCategory is Produkt p)
            {
                newSchablon = expenseController.SaveProductSchablonExpense(p, sp, Amount);
            }
            else if (SelectedSchablon is SchablonkostnadKontor sd)
            {
                newSchablon = expenseController.SaveOfficeSchablonExpense(sd, Amount);
            }
            else if (SelectedSchablon is SchablonkostnadAktivitet sa && SelectedSubCategory is Aktivitet a)
            {
                newSchablon = expenseController.SaveActivitySchablonExpense(a, sa, Amount);
            }
            else
            {
                MessageBox.Show("Felaktig vald kostnad, välj en i tabellen");
                return;
            }

            if (newSchablon is null)
            {
                MessageBox.Show("Gick inte att spara kostnaden");
                return;
            }

            int schablonIndex = SchablonExpenses.IndexOf(SelectedSchablon);

            if (schablonIndex < 0)
            {
                MessageBox.Show("Kan inte hitta kostnaden i tabellen, men sparades korrekt i systemet (ladda om tabellen för att se uppdateringen)");
                return;
            }

            int managedAccountId = SelectedSchablon.Konto.KontoID;
            double managedAmount = Amount;
            SchablonExpenses[schablonIndex] = newSchablon;
            MessageBox.Show($"Kostnad för konto {managedAccountId} sparad med summa {managedAmount}");
        }

        public void Lock()
        {
            if (Locked)
            {
                MessageBox.Show("Schablonkostnaden är redan låst");
                return;
            }

            switch (SelectedCategoryIndex)
            {
                case 0: // kontor
                    expenseController.LockOfficeSchablon();
                    Locked = true;
                    MessageBox.Show("Schablonkostnaden för kontoret är nu låst");
                    break;
                case 1: // aktivitet
                    expenseController.LockActivityDirectExpense();
                    Locked = true;
                    MessageBox.Show("Direkt kostnad för aktiviteter är nu låst");
                    break;
                case 2: // produkt
                    expenseController.LockProductDirectExpense();
                    Locked = true;
                    MessageBox.Show("Direkt kostnad för produkter är nu låst");
                    break;
            }
        }
    }
}
