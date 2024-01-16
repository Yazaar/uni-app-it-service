using BusinessLayer;
using Models;
using PresentationLayer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PresentationLayer.ViewModel.UserControls
{
    public class ResultsViewModel : BaseViewModel
    {
        private readonly CalculatorController calculatorController = new CalculatorController();
        private readonly ResultsController resultsController = new ResultsController();
        private readonly DepartmentController departmentController = new DepartmentController();
        private readonly ProductController productController = new ProductController();
        private readonly ExcelController excelController = new ExcelController();

        public IEnumerable<string> Categories { get; } = new string[] { "Avdelning", "Kontor", "Produkt", "Produktgrupp" };

        private int selectedCategoryIndex = -1;
        public int SelectedCategoryIndex { get => selectedCategoryIndex; set { selectedCategoryIndex = value; OnPropertyChanged(); LoadSubCategories(); } }

        private IEnumerable<object>? subCategories;
        public IEnumerable<object>? SubCategories { get => subCategories; set { subCategories = value; OnPropertyChanged(); } }

        private object? selectedSubCategory;
        public object? SelectedSubCategory { get => selectedSubCategory; set { selectedSubCategory = value; OnPropertyChanged(); LoadResults(); } }

        public IEnumerable<Låsning> Locks { get; set; }

        private double revenue;
        public double Revenue { get => revenue; set { revenue = value; OnPropertyChanged(); OnPropertyChanged("Result"); } }

        private double costs;
        public double Costs { get => costs; set { costs = value; OnPropertyChanged(); OnPropertyChanged("Result"); } }

        public double Result { get => Revenue - Costs; }

        public ICommand BackCommand { get; }
        public ICommand ExportExcelCommand { get; }

        public ResultsViewModel(MainViewModel mainViewModel)
        {
            Locks = resultsController.GetAllLocks();

            BackCommand = new RelayCommand(mainViewModel.Back);
            ExportExcelCommand = new RelayCommand(ExportExcel);
        }

        public void LoadSubCategories()
        {
            switch (SelectedCategoryIndex)
            {
                case 0:
                    SubCategories = departmentController.GetAllDepartments();
                    break;
                case 1:
                    SubCategories = null;
                    LoadResults();
                    break;
                case 2:
                    SubCategories = productController.GetAllProducts();
                    break;
                case 3:
                    SubCategories = productController.GetAllProductGroups();
                    break;
                default:
                    SubCategories = null;
                    break;
            }
        }

        public void LoadResults()
        {
            if (SelectedCategoryIndex == 1) LoadOfficeResults();
            else if (SelectedSubCategory is Produkt p) LoadProductResults(p);
            else if (SelectedSubCategory is Produktgrupp pg) LoadProductGroupResults(pg);
            else if (SelectedSubCategory is Avdelning d) LoadDepartmentResults(d);
        }

        public void LoadProductResults(Produkt product)
        {
            Tuple<double, double> parsed = calculatorController.CalculateProduct(product);
            Revenue = Math.Ceiling(parsed.Item1);
            Costs = Math.Ceiling(parsed.Item2);
        }

        public void LoadProductGroupResults(Produktgrupp productGroup)
        {
            Tuple<double, double> parsed = calculatorController.CalculateProductGroup(productGroup);
            Revenue = Math.Ceiling(parsed.Item1);
            Costs = Math.Ceiling(parsed.Item2);
        }

        public void LoadDepartmentResults(Avdelning department)
        {
            Tuple<double, double> parsed = calculatorController.CalculateDepartment(department);
            Revenue = Math.Ceiling(parsed.Item1);
            Costs = Math.Ceiling(parsed.Item2);
        }

        public void LoadOfficeResults()
        {
            Tuple<double, double> parsed = calculatorController.CalculateOrganisation();
            Revenue = Math.Ceiling(parsed.Item1);
            Costs = Math.Ceiling(parsed.Item2);
        }

        public void ExportExcel()
        {
            string exp;
            if (SelectedSubCategory is Produkt p) exp = excelController.ExportBudgetResult(p.Produktnamn, Revenue, Costs);
            else if (SelectedSubCategory is Avdelning a) exp = excelController.ExportBudgetResult(a.Benämning, Revenue, Costs);
            else if (SelectedSubCategory is Produktgrupp pg) exp = excelController.ExportBudgetResult(pg.Benämning, Revenue, Costs);
            else if (SelectedCategoryIndex == 1) exp = excelController.ExportBudgetResult("Kontor", Revenue, Costs);
            else
            {
                MessageBox.Show("Var god välj data att visa innan export till excel");
                return;
            }

            if (exp is null)
            {
                MessageBox.Show("Gick inte att exportera, är excel installerat? Krävs för excel export");
                return;
            }
        }
    }
}
