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
    public class SystemadministrationViewModel : BaseViewModel
    {
        private readonly RolesController rolesController = new RolesController();

        public ObservableCollection<Roll> Roles { get; }
        public IEnumerable<RollBehörighet> RolePermissions { get; }

        private Roll? selectedRole;
        public Roll? SelectedRole { get => selectedRole; set { selectedRole = value; OnPropertyChanged(); OnPropertyChanged("ActionButtonText"); UpdateFields(); } }

        private RollBehörighet? selectedRolePermission;
        public RollBehörighet? SelectedRolePermission { get => selectedRolePermission; set { selectedRolePermission = value; OnPropertyChanged(); } }

        private string roleName = string.Empty;
        public string RoleName { get => roleName; set { roleName = value; OnPropertyChanged(); } }

        private string roleUsername = string.Empty;
        public string RoleUsername { get => roleUsername; set { roleUsername = value; OnPropertyChanged(); } }

        private string rolePassword = string.Empty;
        public string RolePassword { get => rolePassword; set { rolePassword = value; OnPropertyChanged(); } }

        public string ActionButtonText { get { return SelectedRole is null ? "Lägg till" : "Spara förändringar"; } }

        public ICommand BackCommand { get; }
        public ICommand DeleteRoleCommand { get; }
        public ICommand CreateRoleCommand { get; }

        public SystemadministrationViewModel(MainViewModel mainViewModel)
        {
            Roles = new ObservableCollection<Roll>(rolesController.GetAllRoles());
            RolePermissions = rolesController.GetAllRolePermissions();

            BackCommand = new RelayCommand(mainViewModel.Back);
            DeleteRoleCommand = new RelayCommand(DeleteRole);
            CreateRoleCommand = new RelayCommand(CreateRole);
        }

        public void UpdateFields()
        {
            RolePassword = string.Empty;
            if (SelectedRole is null)
            {
                RoleName = string.Empty;
                RoleUsername = string.Empty;
                SelectedRolePermission = null;
            }
            else
            {
                RoleName = SelectedRole.Benämning;
                RoleUsername = SelectedRole.Användarnamn;
                SelectedRolePermission = RolePermissions.FirstOrDefault(rp => rp.RollBehörighetID == SelectedRole.RollBehörighet.RollBehörighetID);
            }
        }

        public void DeleteRole()
        {
            if (SelectedRole is null) return;
            rolesController.DeleteRole(SelectedRole);
            Roles.Remove(SelectedRole);
            MessageBox.Show("Roll raderad");
        }

        public void CreateRole()
        {
            if (RoleName.Length == 0)
            {
                MessageBox.Show("Var god fyll i ett rollnamn");
                return;
            }
            if (RoleUsername.Length == 0)
            {
                MessageBox.Show("Var god fyll i ett användarnamn");
                return;
            }
            if (RolePassword.Length == 0 && SelectedRole is null)
            {
                MessageBox.Show("Var god fyll i ett lösenord");
                return;
            }
            if (SelectedRolePermission is null)
            {
                MessageBox.Show("Var god välj en behörighet");
                return;
            }

            Roll createdRole;

            if (SelectedRole is null)
            {
                createdRole = rolesController.CreateRole(RoleName, RoleUsername, RolePassword, SelectedRolePermission);
            }
            else
            {
                createdRole = rolesController.UpdateRole(SelectedRole, RoleName, RoleUsername, RolePassword, SelectedRolePermission);
            }

            if (createdRole is null)
            {
                MessageBox.Show("Rollens användarnamn är upptaget, var god välj ett alterativt användarnamn");
                return;
            }

            Roles.Add(createdRole);

            UpdateFields();
            if (SelectedRole is null)
            {
                MessageBox.Show("Roll skapad");
            }
            else
            {
                Roles.Remove(SelectedRole);
                MessageBox.Show("Roll uppdaterad");
            }
        }
    }
}
