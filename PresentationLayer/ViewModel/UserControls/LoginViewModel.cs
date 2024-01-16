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
    public class LoginViewModel : BaseViewModel
    {
        private readonly RolesController userController = new RolesController();

        private readonly MainViewModel mainViewModel;

        private string username = string.Empty;
        public string Username { get => username; set { username = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; }

        public LoginViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            LoginCommand = new LoginCommand(this);
        }

        public void Login(string password)
        {
            Roll role = userController.Login(Username, password);
            if (role is null)
            {
                MessageBox.Show("Felaktiga inloggningsuppgifter");
                return;
            }

            mainViewModel.CurrentRole = role;

            mainViewModel.OpenView(new MenuViewModel(mainViewModel));
        }
    }
}
