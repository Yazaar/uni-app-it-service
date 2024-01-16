using BusinessLayer;
using Microsoft.Win32;
using Models;
using PresentationLayer.Internals;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class PrognosisViewModel : BaseViewModel
    {
        private readonly PrognosisController prognosisController = new PrognosisController();

        private readonly NotifyCollectionChangedEventHandler collectionChangedHandler;
        
        public ViewPermission EditPrognosisPermission { get; }
        public ViewPermission LockPrognosisPermission { get; }

        private ObservableCollection<Prognos> prognosises = new ObservableCollection<Prognos>();
        public ObservableCollection<Prognos> Prognosises
        {
            get => prognosises;
            set
            {
                prognosises.CollectionChanged -= collectionChangedHandler;
                prognosises = value;
                prognosises.CollectionChanged += collectionChangedHandler;
                OnPropertyChanged();
                UpdateSums();
            }
        }

        private IEnumerable<PrognosMånad> prognosisMonths;
        public IEnumerable<PrognosMånad> PrognosisMonths { get => prognosisMonths; set { prognosisMonths = value; OnPropertyChanged(); } }

        private PrognosMånad? selectedMonth;
        public PrognosMånad? SelectedMonth { get => selectedMonth; set { selectedMonth = value; OnPropertyChanged(); LoadPrognosises(); } }

        private bool monthLocked = false;
        public bool MonthLocked { get => monthLocked; set { monthLocked = value; OnPropertyChanged(); OnPropertyChanged("MonthLockedText"); } }

        private bool allLocked = false;
        public bool AllLocked { get => allLocked; set { allLocked = value; OnPropertyChanged(); OnPropertyChanged("AllLockedText"); } }

        public string MonthLockedText { get => MonthLocked ? "Fastställd" : "Ej fastställd"; }
        public string AllLockedText { get => AllLocked ? "Fastställd" : "Ej fastställd"; }

        private Prognos? selectedPrognosis;
        public Prognos? SelectedPrognosis { get => selectedPrognosis; set { selectedPrognosis = value; OnPropertyChanged(); UpdateProps(); } }

        private double reprocessed;
        public double Reprocessed { get => reprocessed; set { reprocessed = value; OnPropertyChanged(); } }

        private double currentPrognosis;
        public double CurrentPrognosis { get => currentPrognosis; set { currentPrognosis = value; OnPropertyChanged(); } }

        private double budgetSum;
        public double BudgetSum {get => budgetSum; set { budgetSum = value; OnPropertyChanged(); }}

        private double monthProfitsSum;
        public double MonthProfitsSum {get => monthProfitsSum; set { monthProfitsSum = value; OnPropertyChanged(); }}

        private double pastYearProfitsSum;
        public double PastYearProfitsSum {get => pastYearProfitsSum; set { pastYearProfitsSum = value; OnPropertyChanged(); }}

        private double reprocessedSum;
        public double ReprocessedSum {get => reprocessedSum; set { reprocessedSum = value; OnPropertyChanged(); }}

        private double trendSum;
        public double TrendSum {get => trendSum; set { trendSum = value; OnPropertyChanged(); }}

        private double prevProgSum;
        public double PrevProgSum {get => prevProgSum; set { prevProgSum = value; OnPropertyChanged(); }}

        private double currentProgSum;
        public double CurrentProgSum {get => currentProgSum; set { currentProgSum = value; OnPropertyChanged(); }}

        private double progBudgetSum;
        public double ProgBudgetSum {get => progBudgetSum; set { progBudgetSum = value; OnPropertyChanged(); }}

        public ICommand BackCommand { get; }
        public ICommand SavePrognosisCommand { get; }
        public ICommand LockMonthCommand { get; }
        public ICommand ImportRevenueFileCommand { get; }

        public PrognosisViewModel(MainViewModel mainViewModel)
        {
            EditPrognosisPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "ekonchef", "fsgmarkchef", "pers" });
            LockPrognosisPermission = new ViewPermission(mainViewModel.CurrentRole, new string[] { "systemans", "ekonchef", "fsgmarkchef" });

            collectionChangedHandler = new NotifyCollectionChangedEventHandler((object? sender, NotifyCollectionChangedEventArgs e) => UpdateSums());

            prognosisMonths = prognosisController.GetAllMonths().OrderByDescending(p => p.PrognosMånadID.Date);
            AllLocked = prognosisController.GetFullLock().Låst;

            BackCommand = new RelayCommand(mainViewModel.Back);
            SavePrognosisCommand = new RelayCommand(SavePrognosis);
            LockMonthCommand = new RelayCommand(LockMonth);
            ImportRevenueFileCommand = new RelayCommand(ImportRevenueFile);
        }

        public void LoadPrognosises()
        {
            if (SelectedMonth is null) return;

            Prognosises = new ObservableCollection<Prognos>(prognosisController.GetPrognosisesByMonth(SelectedMonth.PrognosMånadID.Year, SelectedMonth.PrognosMånadID.Month));
            MonthLocked = SelectedMonth.Låst;
        }

        public void UpdateProps()
        {
            if (SelectedPrognosis is null)
            {
                Reprocessed = 0;
                CurrentPrognosis = 0;
            }
            else
            {
                Reprocessed = SelectedPrognosis.Upparbetat;
                CurrentPrognosis = SelectedPrognosis.NuPrognos;
            }
        }

        public void SavePrognosis()
        {
            if (MonthLocked)
            {
                MessageBox.Show("Månaden är låst, går därav inte att uppdatera några prognoser");
                return;
            }

            if (SelectedPrognosis is null)
            {
                MessageBox.Show("Var god välj en prognos som du vill uppdatera");
                return;
            }


            Prognos updatedPrognosis = prognosisController.UpdatePrognosis(SelectedPrognosis, Reprocessed, CurrentPrognosis);
            if (updatedPrognosis is null)
            {
                MessageBox.Show("Gick inte att uppdatera prognosen");
                return;
            }

            int selectedPrognosisIndex = Prognosises.IndexOf(SelectedPrognosis);
            if (selectedPrognosisIndex == -1)
            {
                MessageBox.Show("Hittade inte prognosen i tabellen och går därför inte att visa förändringen, men prognos sparad");
                return;
            }

            Prognosises[selectedPrognosisIndex] = updatedPrognosis;

            MessageBox.Show("Prognos uppdaterad");
        }

        public void LockMonth()
        {
            if (MonthLocked)
            {
                MessageBox.Show("Månaden är redan låst");
                return;
            }

            if (AllLocked)
            {
                MessageBox.Show("Uppföljning och prognostisering är redan låst och kan inte ändras");
                return;
            }

            if (SelectedMonth is null)
            {
                if (prognosisController.LockMonth(null) == 2)
                {
                    AllLocked = true;
                    MessageBox.Show("Hela uppföljning och prognostisering är nu låst");
                } else MessageBox.Show("Var god välj en månad att låsa");
                return;
            }

            int didLock = prognosisController.LockMonth(SelectedMonth);

            if (didLock == 0)
            {
                MessageBox.Show("Misslyckades att låsa månaden");
                return;
            }

            SelectedMonth.Låst = true;
            MonthLocked = true;

            if (didLock == 1) MessageBox.Show("Månaden är nu låst");
            else if (didLock == 2)
            {
                AllLocked = true;
                MessageBox.Show("Hela uppföljning och prognostisering är nu låst");
            }
        }

        public void UpdateSums()
        {
            double newBudgetSum = 0d;
            double newMonthProfitsSum = 0d;
            double newPastYearProfitsSum = 0d;
            double newReprocessedSum = 0d;
            double newTrendSum = 0d;
            double newPrevProgSum = 0d;
            double newCurrentProgSum = 0d;
            double newProgBudgetSum = 0d;

            foreach (Prognos prognosis in Prognosises)
            {
                newBudgetSum += prognosis.Budget;
                newMonthProfitsSum += prognosis.UtfallMån;
                newPastYearProfitsSum += prognosis.UtfallAcc;
                newReprocessedSum += prognosis.Upparbetat;
                newTrendSum += prognosis.Trend;
                newPrevProgSum += prognosis.FörgProg;
                newCurrentProgSum += prognosis.NuPrognos;
                newProgBudgetSum += prognosis.ProgBudget;
            }

            BudgetSum = newBudgetSum;
            MonthProfitsSum = newMonthProfitsSum;
            PastYearProfitsSum = newPastYearProfitsSum;
            ReprocessedSum = newReprocessedSum;
            TrendSum = newTrendSum;
            PrevProgSum = newPrevProgSum;
            CurrentProgSum = newCurrentProgSum;
            ProgBudgetSum = newProgBudgetSum;
        }

        public void ImportRevenueFile()
        {
            if (AllLocked)
            {
                MessageBox.Show("Uppföljning och prognostisering är låst, kan därför inte importera och ändra");
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt filer (*.txt)|*.txt";
            openFileDialog.Multiselect = false;
            openFileDialog.ShowDialog();
            string selectedFile = openFileDialog.FileName;

            prognosisController.ImportRevenueFile(selectedFile);

            PrognosisMonths = prognosisController.GetAllMonths().OrderByDescending(p => p.PrognosMånadID.Date);
        }
    }
}
