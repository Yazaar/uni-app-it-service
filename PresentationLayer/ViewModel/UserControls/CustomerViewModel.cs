using BusinessLayer;
using Models;
using PresentationLayer.Internals;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class CustomerViewModel : BaseViewModel
    {
        private readonly CustomerController customerController = new CustomerController();

        public ViewPermission AddCustomerPermission { get; }
        public ViewPermission EditCustomerPermission { get; }

        public ObservableCollection<Kund> Customers { get; }
        public ICollection<Kundkategori> CustomerCategories { get; }

        private string customerID = string.Empty;
        public string CustomerID
        {
            get => customerID;
            set
            {
                if (value.Length <= 4) customerID = value;
                OnPropertyChanged();
            }
        }

        private string customerName = string.Empty;
        public string CustomerName { get => customerName; set { customerName = value; OnPropertyChanged(); } }

        private Kundkategori? customerCategory;
        public Kundkategori? CustomerCategory { get => customerCategory; set { customerCategory = value; OnPropertyChanged(); } }

        private Kund? editedCustomer;
        public Kund? EditedCustomer
        {
            get => editedCustomer;
            set
            {
                if (!EditCustomerPermission.HasPermission) return;
                editedCustomer = value;
                OnPropertyChanged(); OnPropertyChanged("CustomerIDWritable"); OnPropertyChanged("ActionButtonText");
                UpdateFields();
            }
        }

        public string ActionButtonText { get { return EditedCustomer is null ? "Lägg till ny" : "Spara förändringar"; } }
        public bool CustomerIDWritable { get { return EditedCustomer is not null; } }

        public ICommand BackCommand { get; }
        public ICommand CreateCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }

        public CustomerViewModel(MainViewModel mainViewModel)
        {
            AddCustomerPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "fsgmarkchef" });
            EditCustomerPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans" });

            Customers = new ObservableCollection<Kund>(customerController.GetAllCustomers());
            CustomerCategories = customerController.GetAllCustomerCategories();

            UpdateFields();

            BackCommand = new RelayCommand(mainViewModel.Back);
            CreateCustomerCommand = new RelayCommand(CreateCustomer);
            DeleteCustomerCommand = new RelayCommand(DeleteCustomer);
        }

        public void UpdateFields()
        {
            if (EditedCustomer is null)
            {
                CustomerID = string.Empty;
                CustomerName = string.Empty;
                CustomerCategory = null;
            }
            else
            {
                CustomerID = EditedCustomer.KundID;
                CustomerName = EditedCustomer.Kundnamn;
                CustomerCategory = CustomerCategories.FirstOrDefault(cc => cc.KundkategoriID == EditedCustomer.Kundkategori.KundkategoriID);
            }
        }

        public void DeleteCustomer()
        {
            if (EditedCustomer is null)
            {
                MessageBox.Show("Välj en kund som du vill radera");
                return;
            }

            customerController.DeleteCustomer(EditedCustomer);
            Customers.Remove(EditedCustomer);
            MessageBox.Show("Kund raderad");
        }

        public void CreateCustomer()
        {
            if (CustomerID.Length != 4)
            {
                MessageBox.Show("Var god fyll i ett kundID som är 4 tecken långt");
                return;
            }
            if (CustomerName.Length == 0)
            {
                MessageBox.Show("Var god fyll i ett kundnamn");
                return;
            }
            if (CustomerCategory is null)
            {
                MessageBox.Show("Var god specifiera en kundkategori");
                return;
            }

            if (EditedCustomer is not null)
            {
                Kund updatedCustomer = customerController.UpdateCustomer(EditedCustomer, CustomerName, CustomerCategory);
                int customerIndex = Customers.IndexOf(EditedCustomer);
                if (customerIndex != -1) Customers[customerIndex] = updatedCustomer;
                UpdateFields();
                MessageBox.Show("Kund uppdaterad");
            }
            else
            {
                Kund createdCustomer = customerController.CreateCustomer(CustomerID, CustomerName, CustomerCategory);
                if (createdCustomer is null)
                {
                    MessageBox.Show("Specifiera ett alternativt kundid (finns redan)");
                    return;
                }
                Customers.Add(createdCustomer);
                UpdateFields();
                MessageBox.Show("Kund skapad");
            }

        }
    }
}
