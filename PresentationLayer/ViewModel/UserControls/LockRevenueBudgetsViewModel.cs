using BusinessLayer;
using Models;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class LockRevenueBudgetsViewModel : BaseViewModel
    {
        public RevenueBudgetsController revenueBudgetsController = new RevenueBudgetsController();

        public IEnumerable<Intäktsbudgetering> RevenueBudgets { get; } = new List<Intäktsbudgetering>();

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
        public ICommand LockCommand { get; }
        public ICommand ExportTXTCommand { get; }

        public LockRevenueBudgetsViewModel(MainViewModel mainViewModel)
        {
            RevenueBudgets = revenueBudgetsController.GetAllRevenueBudgets();
            Locked = revenueBudgetsController.RevenueBudgetsIsLocked();

            UpdateSums();

            BackCommand = new RelayCommand(mainViewModel.Back);
            LockCommand = new RelayCommand(Lock);
            ExportTXTCommand = new RelayCommand(ExportTXT);
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

        public void Lock()
        {
            if (Locked)
            {
                MessageBox.Show("Intäktsbudgeten är redan låst");
                return;
            }

            revenueBudgetsController.LockRevenueBudgets();
            Locked = true;
            MessageBox.Show("Intäktsbudgeten är nu låst");
        }

        public void ExportTXT()
        {
            string filePath = revenueBudgetsController.ExportRevenueBudgetsTXT(RevenueBudgets);
            Process.Start("notepad.exe", filePath);
        }
    }
}
