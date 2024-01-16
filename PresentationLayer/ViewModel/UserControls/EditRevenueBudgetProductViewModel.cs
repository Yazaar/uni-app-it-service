using BusinessLayer;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PresentationLayer.ViewModel.UserControls
{
    public class EditRevenueBudgetProductViewModel : NewRevenueBudgetProductViewModel
    {
        private readonly Intäktsbudgetering revenueBudget;

        public EditRevenueBudgetProductViewModel(MainViewModel mainViewModel, ObservableCollection<Intäktsbudgetering> revenueBudgets, Intäktsbudgetering revenueBudget)
            : base(mainViewModel, revenueBudgets, revenueBudget.Produkt)
        {
            this.revenueBudget = revenueBudget;

            SelectedCustomer = Customers.FirstOrDefault(c => c.KundID == revenueBudget.Kund.KundID);
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

            Intäktsbudgetering intäktsbudgetering = revenueBudgetsController.UpdateRevenueBudget(revenueBudget, SelectedCustomer, product, Agreement, GradeA, Addon, GradeT, Hours, Comment);

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
