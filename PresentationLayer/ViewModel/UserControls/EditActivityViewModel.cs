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

    public class EditActivityViewModel : NewActivityViewModel 
    {
        private readonly Aktivitet activity;

        public EditActivityViewModel(MainViewModel mainViewModel, Aktivitet activity, ObservableCollection<Aktivitet> activities)
            : base(mainViewModel, activities)
        {
            this.activity = activity;

            ActivityID = activity.AktivitetID.Substring(0, 4);
            ActivityName = activity.Benämning;
            Departments = Departments.Where(d => d.AvdelningID == activity.Avdelning.AvdelningID);
            Department = Departments.FirstOrDefault();
        }

        override public void SaveActivity()
        {
            if (Department is null)
            {
                MessageBox.Show("Var god välj en avdelning");
                return;
            }
            if (ActivityName.Length == 0)
            {
                MessageBox.Show("Var god välj ett aktivitetsnamn");
                return;
            }

            Aktivitet createdActivity = activityController.UpdateActivity(activity, ActivityName);

            if (createdActivity is null) return;

            int activityIndex = activities.IndexOf(activity);
            if (activityIndex != -1)
            {
                activities[activityIndex] = createdActivity;
            }

            mainViewModel.Back();
        }
    }
}
