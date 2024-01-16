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
    public class EditProductViewModel : NewProductViewModel
    {
        private Produkt product;

        public EditProductViewModel(MainViewModel mainViewModel, Produkt product, ObservableCollection<Produkt> products)
            : base(mainViewModel, products)
        {
            this.product = product;

            ProductID = product.ProduktID.Substring(0, 4);
            ProductName = product.Produktnamn;
            Department = Departments.FirstOrDefault(d => d.AvdelningID == product.Avdelning.AvdelningID);
            ProductGroups = ProductGroups.Where(pg => pg.ProduktgruppID == product.Produktgrupp.ProduktgruppID);
            ProductGroup = ProductGroups.FirstOrDefault();
        }

        override public void SaveProduct()
        {
            Produkt updatedProduct = productController.UpdateProduct(product, ProductName, Department);

            if (updatedProduct is null) return;

            int productIndex = products.IndexOf(product);
            if (productIndex != -1)
            {
                products[productIndex] = updatedProduct;
            }

            mainViewModel.Back();
        }
    }
}
