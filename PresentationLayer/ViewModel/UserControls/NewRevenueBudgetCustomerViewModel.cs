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
    public class NewRevenueBudgetCustomerViewModel : BaseViewModel
    {
        protected readonly MainViewModel mainViewModel;
        protected readonly ObservableCollection<Intäktsbudgetering> revenueBudgets;
        protected readonly Kund customer;

        protected readonly ProductController productController = new ProductController();
        protected readonly RevenueBudgetsController revenueBudgetsController = new RevenueBudgetsController();

        public IEnumerable<string> Grades { get; } = new string[] { "Säker", "Inte säker" };

        public IEnumerable<Produkt> Products { get; }

        private Produkt? selectedProduct;
        public Produkt? SelectedProduct { get => selectedProduct; set { selectedProduct = value; OnPropertyChanged(); } }

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

        public NewRevenueBudgetCustomerViewModel(MainViewModel mainViewModel, ObservableCollection<Intäktsbudgetering> revenueBudgets, Kund customer)
        {
            this.mainViewModel = mainViewModel;
            this.revenueBudgets = revenueBudgets;
            this.customer = customer;

            Products = productController.GetAllProducts().OrderBy(p => p.Produktnamn);

            BackCommand = new RelayCommand(mainViewModel.Back);
            SaveCommand = new RelayCommand(Save);
        }

        public bool Valid()
        {
            if (SelectedProduct is null)
            {
                MessageBox.Show("Var god välj en produkt");
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

            Intäktsbudgetering intäktsbudgetering = revenueBudgetsController.CreateRevenueBudget(customer, SelectedProduct, Agreement, GradeA, Addon, GradeT, Hours, Comment);
            
            if (intäktsbudgetering is null)
            {
                MessageBox.Show("Misslyckades att skapa en intäktsbudgetering för kunden");
                return;
            }

            revenueBudgets.Add(intäktsbudgetering);
            mainViewModel.Back();

        }
    }
}
