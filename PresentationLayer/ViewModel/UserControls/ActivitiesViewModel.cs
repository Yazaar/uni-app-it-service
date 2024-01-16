using BusinessLayer;
using Models;
using PresentationLayer.Internals;
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
    public class ActivitiesViewModel : BaseViewModel
    {
        private readonly MainViewModel mainViewModel;

        public ViewPermission NewActivityPermission { get; }
        public ViewPermission EditActivityPermission { get; }

        private readonly ActivityController activityController = new ActivityController();

        public ObservableCollection<Aktivitet> Activities { get; }

        private Aktivitet? selectedActivity;
        public Aktivitet? SelectedActivity { get => selectedActivity; set { selectedActivity = value; OnPropertyChanged(); } }

        public ICommand BackCommand { get; }
        public ICommand OpenNewActivityCommand { get; }
        public ICommand OpenEditActivityCommand { get; }
        public ICommand DeleteActivityCommand { get; }

        public ActivitiesViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;

            NewActivityPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "fsgmarkchef", "admchef" });
            EditActivityPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans" });

            Activities = new ObservableCollection<Aktivitet>(activityController.GetAllActivities());

            BackCommand = new RelayCommand(mainViewModel.Back);
            OpenNewActivityCommand = new RelayCommand(() => mainViewModel.OpenView(new NewActivityViewModel(mainViewModel, Activities)));
            OpenEditActivityCommand = new RelayCommand(EditActivity);
            DeleteActivityCommand = new RelayCommand(DeleteActivity);
        }

        public void DeleteActivity()
        {
            if (SelectedActivity is null) return;

            activityController.DeleteActivity(SelectedActivity);
            Activities.Remove(SelectedActivity);
            MessageBox.Show("Aktivitet raderad");
        }

        public void EditActivity()
        {
            if (SelectedActivity is not null) mainViewModel.OpenView(new EditActivityViewModel(mainViewModel, SelectedActivity, Activities));
        }
    }
}
