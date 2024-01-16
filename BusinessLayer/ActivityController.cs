using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class ActivityController
    {
        public IEnumerable<Avdelning> GetAllActivityDepartments()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.DepartmentRepository.FindAll(d => d.Avdelningstyp.Benämning == "AFFO", d => d.Avdelningstyp);
            }
        }

        public IEnumerable<Aktivitet> GetAllActivities()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ActivityRepository.FindAll(a => a.Avdelning).ToList();
            }
        }

        public IEnumerable<Aktivitetfördelning> GetAllActivityDistributionsByActivity(Aktivitet activity)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ActivityDistributionRepository.FindAll(
                    ad => ad.Aktivitet.AktivitetID == activity.AktivitetID,
                    a => a.Aktivitet, a => a.Avdelningfördelning, a => a.Avdelningfördelning.Personal
                    );
            }
        }

        public Aktivitet CreateActivity(string activityID, string activityName, Avdelning department)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Avdelning departmentDB = unitOfWork.DepartmentRepository.Find(d => d.AvdelningID == department.AvdelningID);
                if (departmentDB is null) return null;

                string actualActivityID = activityID + department.AvdelningID;

                if (unitOfWork.ActivityRepository.Find(a => a.AktivitetID == actualActivityID) is not null)
                {
                    return null;
                }

                Aktivitet newActivity = new Aktivitet()
                {
                    AktivitetID = actualActivityID,
                    Benämning = activityName,
                    Avdelning = departmentDB
                };

                unitOfWork.ActivityRepository.Add(newActivity);
                unitOfWork.Save();

                return newActivity;
            }
        }

        public Aktivitet UpdateActivity(Aktivitet activity, string activityName)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Aktivitet activityDB = unitOfWork.ActivityRepository.Find(a => a.AktivitetID == activity.AktivitetID, a => a.Avdelning);
                if (activityDB is null) return null;

                activityDB.Benämning = activityName;

                unitOfWork.Save();

                return activityDB;
            }
        }

        public void DeleteActivity(Aktivitet activity)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Aktivitet activityDB = unitOfWork.ActivityRepository.Find(a => a.AktivitetID == activity.AktivitetID);
                if (activityDB is null) return;
                unitOfWork.ActivityRepository.Remove(activityDB);
                unitOfWork.Save();
            }
        }

        public IEnumerable<Aktivitet> GetAllActivitiesByDepartment(Avdelning department)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ActivityRepository.FindAll(a => a.Avdelning.AvdelningID == department.AvdelningID, a => a.Avdelning);
            }
        }

        public IEnumerable<Aktivitetfördelning> GetAllActivityDistributionsByDepartmentEmpty(Avdelning department)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ActivityRepository.FindAll(a => a.Avdelning.AvdelningID == department.AvdelningID, a => a.Avdelning).Select(a => new Aktivitetfördelning() { Aktivitet = a });
            }
        }

        public void LockDepartmentActivityExpenses(Avdelning department)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Avdelning departmentDB = unitOfWork.DepartmentRepository.Find(d => d.AvdelningID == department.AvdelningID);
                if (departmentDB is null) return;
                departmentDB.KostnadsbudgetAktivitetLåst = true;
                unitOfWork.Save();
            }
        }

        public IEnumerable<Aktivitetfördelning> GetAllActivityDistributions()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ActivityDistributionRepository.FindAll(ad => ad.Aktivitet);
            }
        }
    }
}
