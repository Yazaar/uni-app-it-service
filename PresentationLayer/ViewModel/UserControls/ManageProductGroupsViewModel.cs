using Accessibility;
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
    public class ManageProductGroupsViewModel : BaseViewModel
    {
        private readonly ProductController productController = new ProductController();

        public ViewPermission NewProductGroupPermission { get; }
        public ViewPermission EditProductGroupPermission { get; }

        public ObservableCollection<Produktgrupp> ProductGroups { get; }
        public IEnumerable<Produktkategori> ProductCategories { get; }

        private string productGroupID = string.Empty;
        public string ProductGroupID
        {
            get => productGroupID;
            set
            {
                if (value.Length <= 2) productGroupID = value;
                OnPropertyChanged();
            }
        }

        private string newDescription = string.Empty;
        public string NewDescription { get => newDescription; set { newDescription = value; OnPropertyChanged(); } }

        private Produktkategori? selectedProductCategory;
        public Produktkategori? SelectedProductCategory { get => selectedProductCategory; set { selectedProductCategory = value; OnPropertyChanged(); } }

        public Produktgrupp? editedProductGroup;
        public Produktgrupp? EditedProductGroup
        {
            get => editedProductGroup;
            set
            {
                if (!EditProductGroupPermission.HasPermission) return;
                editedProductGroup = value;
                OnPropertyChanged();
                OnPropertyChanged("ProductGroupActionButtonText");
                OnPropertyChanged("ProductGroupIDWritable");
                UpdateFields();
            }
        }

        public string ProductGroupActionButtonText { get { return EditedProductGroup is null ? "Lägg till ny" : "Spara förändringar"; } }
        public bool ProductGroupIDWritable { get { return EditedProductGroup is not null; } }

        public ICommand BackCommand { get; set; }
        public ICommand CreateProductGroupCommand { get; set; }
        public ICommand DeleteProductGroupCommand { get; set; }

        public ManageProductGroupsViewModel(MainViewModel mainViewModel)
        {
            NewProductGroupPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "driftchef", "utvchef" });
            EditProductGroupPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans" });

            ProductCategories = productController.GetAllProductCategories();
            ProductGroups = new ObservableCollection<Produktgrupp>(productController.GetAllProductGroups());

            BackCommand = new RelayCommand(() => mainViewModel.Back());
            CreateProductGroupCommand = new RelayCommand(CreateProductGroup);
            DeleteProductGroupCommand = new RelayCommand(DeleteProductGroup);
        }

        public void UpdateFields()
        {
            if (EditedProductGroup is null)
            {
                NewDescription = string.Empty;
                SelectedProductCategory = null;
                ProductGroupID = string.Empty;
            }
            else
            {
                NewDescription = EditedProductGroup.Benämning;
                SelectedProductCategory = ProductCategories.FirstOrDefault(pc => pc.ProduktkategoriID == EditedProductGroup.Produktkategori.ProduktkategoriID);
                ProductGroupID = EditedProductGroup.ProduktgruppID;
            }
        }
        
        public void DeleteProductGroup()
        {
            if (EditedProductGroup is null)
            {
                MessageBox.Show("Välj en produktgrupp som du vill radera");
                return;
            }

            productController.DeleteProductGroup(EditedProductGroup);
            ProductGroups.Remove(EditedProductGroup);
            MessageBox.Show("Produktgrupp raderad");
        }

        public void CreateProductGroup()
        {
            if (NewDescription.Length == 0)
            {
                MessageBox.Show("Var god fyll i en benämning");
                return;
            }
            if (SelectedProductCategory is null)
            {
                MessageBox.Show("Var god välj en produktkategori");
                return;
            }

            Produktgrupp registeredProductGroup;
            
            if (EditedProductGroup is not null)
            {
                registeredProductGroup = productController.UpdateProductGroup(EditedProductGroup, NewDescription, SelectedProductCategory);
            }
            else
            {
                registeredProductGroup = productController.CreateProductGroup(ProductGroupID, NewDescription, SelectedProductCategory);
            }

            if (registeredProductGroup is null)
            {
                MessageBox.Show("Var god fyll i ett alternativt produktgrupps-id då det redan är upptaget");
                return;
            }
            

            ProductGroups.Add(registeredProductGroup);

            UpdateFields();
            if (EditedProductGroup is null)
            {
                MessageBox.Show("Produktgrupp skapad");
            }
            else
            {
                ProductGroups.Remove(EditedProductGroup);
                MessageBox.Show("Produktgrupp uppdaterad");
            }
        }
    }
}
