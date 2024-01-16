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

namespace PresentationLayer.ViewModel.UserControls
{
    public class NewProductViewModel : BaseViewModel
    {
        protected readonly ProductController productController = new ProductController();

        protected readonly MainViewModel mainViewModel;

        protected readonly ObservableCollection<Produkt> products;

        public IEnumerable<Avdelning> Departments { get; }
        public IEnumerable<Produktgrupp> ProductGroups { get; protected set; }

        private string productID = string.Empty;
        public string ProductID
        {
            get => productID;
            set
            {
                if (value.Length <= 4) productID = value;
                OnPropertyChanged();
            }
        }

        private string productName = string.Empty;
        public string ProductName { get => productName; set { productName = value; OnPropertyChanged(); } }
        
        private Produktgrupp? productGroup;
        public Produktgrupp? ProductGroup { get => productGroup; set { productGroup = value; OnPropertyChanged(); } }
        
        private Avdelning? department;
        public Avdelning? Department { get => department; set { department = value; OnPropertyChanged(); } }
        public string DepartmentVisibility { get => Departments.Count() == 1 ? "Collapsed" : "Visible"; }

        public ICommand BackCommand { get; set; }
        public ICommand SaveProductCommand { get; set; }

        public NewProductViewModel(MainViewModel mainViewModel, ObservableCollection<Produkt> products)
        {
            this.mainViewModel = mainViewModel;
            this.products = products;

            ProductGroups = productController.GetAllProductGroups().OrderBy(pg => pg.Benämning);
            Departments = productController.GetAllProductDepartments();
            
            BackCommand = new RelayCommand(() => mainViewModel.Back());
            SaveProductCommand = new RelayCommand(SaveProduct);

            switch (mainViewModel.CurrentRole?.RollBehörighet.RollBehörighetID)
            {
                case "driftchef":
                    Departments = Departments.Where(a => a.AvdelningID == "DR");
                    break;
                case "utvchef":
                    Departments = Departments.Where(a => a.AvdelningID == "UF");
                    break;
            }

            if (Departments.Count() == 1) Department = Departments.First();
        }

        virtual public void SaveProduct()
        {
            Produkt createdProduct = productController.CreateProduct(ProductID, ProductName, ProductGroup, Department);

            if (createdProduct is null)
            {
                return;
            }

            products.Add(createdProduct);
            mainViewModel.Back();
        }
    }
}
