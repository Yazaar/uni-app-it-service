using BusinessLayer;
using Models;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class RevenueBudgetsByCustomerViewModel : BaseViewModel
    {
        private readonly MainViewModel mainViewModel;
        
        private readonly CustomerController customerController = new CustomerController();
        private readonly RevenueBudgetsController revenueBudgetsController = new RevenueBudgetsController();

        private readonly NotifyCollectionChangedEventHandler collectionChangedHandler;

        public IEnumerable<Kund> Customers { get; set; }

        private Kund? selectedCustomer;
        public Kund? SelectedCustomer { get => selectedCustomer; set { selectedCustomer = value; OnPropertyChanged(); UpdateRevenueBudgets(); } }

        private ObservableCollection<Intäktsbudgetering> revenueBudgets = new ObservableCollection<Intäktsbudgetering>();
        public ObservableCollection<Intäktsbudgetering> RevenueBudgets
        {
            get => revenueBudgets;
            set
            {
                revenueBudgets.CollectionChanged -= collectionChangedHandler;
                revenueBudgets = value;
                revenueBudgets.CollectionChanged += collectionChangedHandler;
                OnPropertyChanged();
                UpdateSums();
            }
        }

        private Intäktsbudgetering? selectedRevenueBudget;
        public Intäktsbudgetering? SelectedRevenueBudget { get => selectedRevenueBudget; set { selectedRevenueBudget = value; OnPropertyChanged(); } }

        double agreementSum;
        public double AgreementSum { get => agreementSum; set { agreementSum = value; OnPropertyChanged(); } }

        double addonSum;
        public double AddonSum { get => addonSum; set { addonSum = value; OnPropertyChanged(); } }

        double budgetSum;
        public double BudgetSum { get => budgetSum; set { budgetSum = value; OnPropertyChanged(); } }

        double hoursSum;
        public double HoursSum { get => hoursSum; set { hoursSum = value; OnPropertyChanged(); } }

        public string LockedText { get => Locked ? "Fastställd" : "Ej fastställd"; }

        private bool locked;
        public bool Locked { get => locked; set { locked = value; OnPropertyChanged(); OnPropertyChanged("LockedText"); } }

        public ICommand BackCommand { get; }
        public ICommand NewRevenueBudgetCommand { get; }
        public ICommand EditRevenueBudgetCommand { get; }
        public ICommand DeleteRevenueBudgetCommand { get; }

        public RevenueBudgetsByCustomerViewModel(MainViewModel mainViewModel)
        {
            collectionChangedHandler = new NotifyCollectionChangedEventHandler((object? sender, NotifyCollectionChangedEventArgs e) => UpdateSums());

            this.mainViewModel = mainViewModel;

            Customers = customerController.GetAllCustomers().OrderBy(c => c.Kundnamn);
            Locked = revenueBudgetsController.RevenueBudgetsIsLocked();

            BackCommand = new RelayCommand(mainViewModel.Back);
            NewRevenueBudgetCommand = new RelayCommand(NewRevenueBudget);
            EditRevenueBudgetCommand = new RelayCommand(EditRevenueBudget);
            DeleteRevenueBudgetCommand = new RelayCommand(DeleteRevenueBudget);

        }

        public void UpdateRevenueBudgets()
        {
            if (SelectedCustomer is null)
            {
                RevenueBudgets.Clear();
                return;
            }

            RevenueBudgets = new ObservableCollection<Intäktsbudgetering>(revenueBudgetsController.GetRevenueBudgetsByCustomer(SelectedCustomer));
        }

        public void NewRevenueBudget()
        {
            if (Locked)
            {
                MessageBox.Show("Intäktsbudgeten är låst, går ej att skapa ny intäktsbudget");
                return;
            }
            if (SelectedCustomer is null)
            {
                MessageBox.Show("Var god välj en kund att skapa en intäktsbudget för");
                return;
            }

            mainViewModel.OpenView(new NewRevenueBudgetCustomerViewModel(mainViewModel, RevenueBudgets, SelectedCustomer));
        }

        public void EditRevenueBudget()
        {
            if (Locked)
            {
                MessageBox.Show("Intäktsbudgeten är låst, går ej att redigera intäktsbudget");
                return;
            }
            if (SelectedRevenueBudget is null)
            {
                MessageBox.Show("Var god välj en intäktsbudget att redigera");
                return;
            }

            mainViewModel.OpenView(new EditRevenueBudgetCustomerViewModel(mainViewModel, RevenueBudgets, SelectedRevenueBudget));
        }

        public void DeleteRevenueBudget()
        {
            if (Locked)
            {
                MessageBox.Show("Intäktsbudgeten är låst, går ej att radera intäktsbudget");
                return;
            }
            if (SelectedRevenueBudget is null)
            {
                MessageBox.Show("Var god välj en intäktsbudget att radera");
                return;
            }

            revenueBudgetsController.DeleteRevenueBudget(SelectedRevenueBudget);
            RevenueBudgets.Remove(SelectedRevenueBudget);
            MessageBox.Show("Intäktsbudget raderad");
        }

        public void UpdateSums()
        {
            double agreementSum = 0, addonSum = 0, budgetSum = 0, hoursSum = 0;

            foreach (Intäktsbudgetering rb in RevenueBudgets)
            {
                agreementSum += rb.Avtal;
                addonSum += rb.Tillägg;
                budgetSum += rb.Budget;
                hoursSum += rb.Tim;
            }

            AgreementSum = agreementSum;
            AddonSum = addonSum;
            BudgetSum = budgetSum;
            HoursSum = hoursSum;
        }
    }
}
