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
    public class NewActivityViewModel : BaseViewModel
    {
        protected readonly MainViewModel mainViewModel;

        protected readonly ActivityController activityController = new ActivityController();
        protected readonly CustomerController customerController = new CustomerController();

        protected readonly ObservableCollection<Aktivitet> activities;

        public IEnumerable<Avdelning> Departments { get; protected set; }
        public IEnumerable<Kund> Customers { get; }

        private string activityID = string.Empty;
        public string ActivityID
        {
            get => activityID;
            set
            {
                if (value.Length <= 4) activityID = value;
                OnPropertyChanged();
            }
        }

        private string activityName = string.Empty;
        public string ActivityName { get => activityName; set { activityName = value; } }

        private Avdelning? department;
        public Avdelning? Department { get => department; set { department = value; } }
        public string DepartmentVisibility { get => Departments.Count() == 1 ? "Collapsed" : "Visible"; }

        public ICommand BackCommand { get; }
        public ICommand SaveActivityCommand { get; }

        public NewActivityViewModel(MainViewModel mainViewModel, ObservableCollection<Aktivitet> activities)
        {
            this.mainViewModel = mainViewModel;
            this.activities = activities;

            Departments = activityController.GetAllActivityDepartments();
            Customers = customerController.GetAllCustomers();

            BackCommand = new RelayCommand(mainViewModel.Back);
            SaveActivityCommand = new RelayCommand(SaveActivity);

            switch (mainViewModel.CurrentRole?.RollBehörighet.RollBehörighetID)
            {
                case "fsgmarkchef":
                    Departments = Departments.Where(a => a.AvdelningID == "FO");
                    break;
                case "admchef":
                    Departments = Departments.Where(a => a.AvdelningID == "AO");
                    break;
            }

            if (Departments.Count() == 1) Department = Departments.First();
        }

        virtual public void SaveActivity()
        {
            if (Department is null)
            {
                MessageBox.Show("Var god välj en avdelning");
                return;
            }
            if (ActivityID.Length != 4)
            {
                MessageBox.Show("Var god välj ett aktivitet ID");
                return;
            }
            if (ActivityName.Length == 0)
            {
                MessageBox.Show("Var god välj ett aktivitetsnamn");
                return;
            }

            Aktivitet createdActivity = activityController.CreateActivity(ActivityID, ActivityName, Department);

            if (createdActivity is null)
            {
                MessageBox.Show("Aktivitet ID:t finns redan, var god specifiera ett annat");
                return;
            }

            activities.Add(createdActivity);
            mainViewModel.Back();
        }
    }
}
