using BusinessLayer;
using Models;
using PresentationLayer.ViewModel.Commands;
using PresentationLayer.ViewModel.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PresentationLayer.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel currentView;
        public BaseViewModel CurrentView { get => currentView; private set { currentView = value; OnPropertyChanged(); } }

        private Roll? currentRole;
        public Roll? CurrentRole { get => currentRole; set { currentRole = value; OnPropertyChanged(); } }

        private Stack<BaseViewModel> viewHistory = new Stack<BaseViewModel>();

        public MainViewModel()
        {
            currentView = new LoginViewModel(this);

            ExcelController excelController = new ExcelController();
            excelController.ImportProducts();
            excelController.ImportCustomers();
            excelController.ImportEmployees();
            excelController.ImportAccounts();
        }

        public void OpenView<T>(T baseViewModel) where T : BaseViewModel
        {
            viewHistory.Push(CurrentView);
            CurrentView = baseViewModel;
        }

        public void Back()
        {
            if (viewHistory.Count > 0)
                CurrentView = viewHistory.Pop();
        }
    }
}
