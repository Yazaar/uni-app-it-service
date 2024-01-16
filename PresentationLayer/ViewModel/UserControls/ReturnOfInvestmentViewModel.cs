using BusinessLayer;
using Models;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class ReturnOfInvestmentViewModel : BaseViewModel
    {
        public ReturnOfInvestmentController returnOfInvestmentController = new ReturnOfInvestmentController();

        private double amount;
        public double Amount { get => amount; set { amount = value; OnPropertyChanged(); } }

        public int AccountNumber { get; } = 9999;

        public string AccountName { get; } = "Avkastningskrav";

        private bool locked;
        public bool Locked { get => locked; set { locked = value; OnPropertyChanged(); OnPropertyChanged("LockedText"); } }
        public string LockedText { get => Locked ? "Fastställd" : "Ej fastställd"; }

        public ICommand BackCommand { get; }
        public ICommand SaveROICommand { get; }
        public ICommand LockROICommand { get; }

        public ReturnOfInvestmentViewModel(MainViewModel mainViewModel)
        {
            BackCommand = new RelayCommand(mainViewModel.Back);
            SaveROICommand = new RelayCommand(SaveROI);
            LockROICommand = new RelayCommand(LockROI);

            Avkastningskrav currentROI = returnOfInvestmentController.GetReturnOfInvestment();
            if (currentROI is not null) Amount = currentROI.Belopp;

            Locked = returnOfInvestmentController.GetLock().Låst;
        }

        public void SaveROI()
        {
            if (Locked)
            {
                MessageBox.Show("Avkastningskravet är låst, kan inte ändra");
                return;
            }

            if (Amount < 0)
            {
                MessageBox.Show("Avkastningskravet kan inte vara negativt, var god ändra");
                return;
            }

            Avkastningskrav setROI = returnOfInvestmentController.SetReturnOfInvestment(Amount);
            
            if (setROI is null)
            {
                MessageBox.Show("Misslyckades att skapa ett avkastningskrav");
                return;
            }

            MessageBox.Show($"Avkastningskrav på {setROI.Belopp}kr sparat");
        }

        public void LockROI()
        {
            if (Locked)
            {
                MessageBox.Show("Avkastningskravet är redan låst");
                return;
            }

            returnOfInvestmentController.LockROI();
            Locked = true;
            MessageBox.Show("Avkastningskravet är nu låst");
        }
    }
}
