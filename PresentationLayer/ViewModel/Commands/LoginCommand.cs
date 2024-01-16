using PresentationLayer.ViewModel.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.Commands
{
    public class LoginCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private readonly LoginViewModel loginViewModel;

        public LoginCommand(LoginViewModel loginViewModel)
        {
            this.loginViewModel = loginViewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (loginViewModel.Username.Length == 0)
            {
                MessageBox.Show("Var god specifiera ett användarnamn");
                return;
            }
            if (parameter is PasswordBox passwordBox)
            {
                var password = passwordBox.Password;
                if (password is null || password.Length == 0)
                {
                    MessageBox.Show("Var god specifiera ett lösenord");
                    return;
                }
                loginViewModel.Login(password);
            }
        }
    }
}
