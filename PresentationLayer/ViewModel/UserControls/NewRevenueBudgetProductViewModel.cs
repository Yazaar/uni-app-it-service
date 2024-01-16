using BusinessLayer;
using Models;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace PresentationLayer.ViewModel.UserControls
{
    public class NewRevenueBudgetProductViewModel : BaseViewModel
    {
        protected readonly MainViewModel mainViewModel;
        protected readonly ObservableCollection<Intäktsbudgetering> revenueBudgets;
        protected readonly Produkt product;

        protected readonly CustomerController customerController = new CustomerController();
        protected readonly RevenueBudgetsController revenueBudgetsController = new RevenueBudgetsController();

        public IEnumerable<string> Grades { get; } = new string[] { "Säker", "Inte säker" };

        public IEnumerable<Kund> Customers { get; }

        private Kund? selectedCustomer;
        public Kund? SelectedCustomer { get => selectedCustomer; set { selectedCustomer = value; OnPropertyChanged(); } }

        private double agreement;
        public double Agreement
        {
            get => agreement;
            set { agreement = value; OnPropertyChanged(); OnPropertyChanged("Budget"); }
        }

        private string? gradeA;
        public string? GradeA
        {
            get => gradeA;
            set { gradeA = value; OnPropertyChanged(); }
        }

        private double addon;
        public double Addon
        {
            get => addon;
            set { addon = value; OnPropertyChanged(); OnPropertyChanged("Budget"); }
        }

        private string? gradeT;
        public string? GradeT
        {
            get => gradeT;
            set { gradeT = value; OnPropertyChanged(); }
        }

        public double Budget { get => Agreement + Addon; }

        private double hours;
        public double Hours
        {
            get => hours;
            set { hours = value; OnPropertyChanged(); }
        }

        private string comment = string.Empty;
        public string Comment
        {
            get => comment;
            set { comment = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }
        public ICommand SaveCommand { get; }

        public NewRevenueBudgetProductViewModel(MainViewModel mainViewModel, ObservableCollection<Intäktsbudgetering> revenueBudgets, Produkt product)
        {
            this.mainViewModel = mainViewModel;
            this.revenueBudgets = revenueBudgets;
            this.product = product;

            Customers = customerController.GetAllCustomers().OrderBy(c => c.Kundnamn);

            BackCommand = new RelayCommand(mainViewModel.Back);
            SaveCommand = new RelayCommand(Save);
        }

        public bool Valid()
        {
            if (SelectedCustomer is null)
            {
                MessageBox.Show("Var god välj en kund");
                return false;
            }

            if (GradeA is null)
            {
                MessageBox.Show("Var god välj en GradA för avtalet");
                return false;
            }

            if (GradeT is null)
            {
                MessageBox.Show("Var god välj en GradT för tillägget");
                return false;
            }

            return true;
        }

        virtual public void Save()
        {
            if (!Valid()) return;

            Intäktsbudgetering intäktsbudgetering = revenueBudgetsController.CreateRevenueBudget(SelectedCustomer, product, Agreement, GradeA, Addon, GradeT, Hours, Comment);

            if (intäktsbudgetering is null)
            {
                MessageBox.Show("Misslyckades att skapa en intäktsbudgetering för produkten");
                return;
            }

            revenueBudgets.Add(intäktsbudgetering);
            mainViewModel.Back();

        }
    }
}
