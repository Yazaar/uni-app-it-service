using BusinessLayer;
using Models;
using PresentationLayer.Internals;
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
    public class ProductViewModel : BaseViewModel
    {
        private readonly MainViewModel mainViewModel;

        private readonly ProductController productController = new ProductController();

        public ViewPermission NewProductPermission { get; }
        public ViewPermission EditProductPermission { get; }

        public ObservableCollection<Produkt> Products { get; }

        private Produkt? selectedProduct;
        public Produkt? SelectedProduct { get => selectedProduct; set { selectedProduct = value; OnPropertyChanged(); } }

        public ICommand BackCommand { get; }
        public ICommand OpenNewProductCommand { get; }
        public ICommand OpenEditProductCommand { get; }
        public ICommand OpenManageProductGroupsCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public ProductViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;

            NewProductPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "driftchef", "utvchef" });
            EditProductPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans" });

            Products = new ObservableCollection<Produkt>(productController.GetAllProducts());

            BackCommand = new RelayCommand(mainViewModel.Back);
            OpenNewProductCommand = new RelayCommand(() => mainViewModel.OpenView(new NewProductViewModel(mainViewModel, Products)));
            OpenEditProductCommand = new RelayCommand(EditProduct);
            OpenManageProductGroupsCommand = new RelayCommand(() => mainViewModel.OpenView(new ManageProductGroupsViewModel(mainViewModel)));
            DeleteProductCommand = new RelayCommand(DeleteProduct);
        }

        public void EditProduct()
        {
            if(SelectedProduct is not null) mainViewModel.OpenView(new EditProductViewModel(mainViewModel, SelectedProduct, Products));
        }

        public void DeleteProduct()
        {
            if (SelectedProduct is null)
            {
                MessageBox.Show("Välj en produkt att radera");
                return;
            }

            productController.DeleteProduct(SelectedProduct);
            Products.Remove(SelectedProduct);
            MessageBox.Show("Produkt raderad");
        }
    }
}
