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
    public class EditRevenueBudgetCustomerViewModel : NewRevenueBudgetCustomerViewModel
    {
        private readonly Intäktsbudgetering revenueBudget;

        public EditRevenueBudgetCustomerViewModel(MainViewModel mainViewModel, ObservableCollection<Intäktsbudgetering> revenueBudgets, Intäktsbudgetering revenueBudget)
            : base (mainViewModel, revenueBudgets, revenueBudget.Kund)
        {
            this.revenueBudget = revenueBudget;

            SelectedProduct = Products.FirstOrDefault(p => p.ProduktID == revenueBudget.Produkt.ProduktID);
            Agreement = revenueBudget.Avtal;
            GradeA = revenueBudget.GradA;
            Addon = revenueBudget.Tillägg;
            GradeT = revenueBudget.GradT;
            Hours = revenueBudget.Tim;
            Comment = revenueBudget.Kommentar;
        }

        public override void Save()
        {
            if (!Valid()) return;

            Intäktsbudgetering intäktsbudgetering = revenueBudgetsController.UpdateRevenueBudget(revenueBudget, customer, SelectedProduct, Agreement, GradeA, Addon, GradeT, Hours, Comment);

            if (intäktsbudgetering is null)
            {
                MessageBox.Show("Misslyckades att skapa en intäktsbudgetering för kunden");
                return;
            }

            revenueBudgets.Remove(revenueBudget);
            revenueBudgets.Add(intäktsbudgetering);

            mainViewModel.Back();
        }
    }
}
