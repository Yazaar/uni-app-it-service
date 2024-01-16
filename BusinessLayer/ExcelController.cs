using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class ExcelController
    {
        private static readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string budgetResultDir = @$"{baseDir}BudgeteratResultat";
        private static readonly string productsDir = $"{baseDir}Produkter";
        private static readonly string productsHandledDir = $@"{productsDir}\Hanterade";
        private static readonly string customersDir = $"{baseDir}Kunder";
        private static readonly string customersHandledDir = $@"{customersDir}\Hanterade";
        private static readonly string employeesDir = $"{baseDir}Personal";
        private static readonly string employeesHandledDir = $@"{employeesDir}\Hanterade";
        private static readonly string accountsDir = $"{baseDir}Konton";
        private static readonly string accountsHandledDir = $@"{accountsDir}\Hanterade";

        private Dictionary<string, string> GetExcelHeaders(ExcelFile excelFile)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            int currentColumn = 0;

            while (true)
            {
                string columnName = ExcelFile.IntToColumn(++currentColumn);
                string cellValue = excelFile.ReadCell<string>(columnName, 1);
                if (cellValue is null || cellValue.Length == 0) break;

                headers[cellValue.ToLower()] = columnName;
            }

            return headers;
        }

        private bool ImportProductFile(string productFile, UnitOfWork unitOfWork, List<string> products, List<Produktgrupp> productGroups,
            IEnumerable<Produktkategori> possibleCategories, IEnumerable<Avdelning> possibleDepartments)
        {
            using (ExcelFile excelFile = new ExcelFile(productFile, true, false))
            {
                if (!excelFile.LoadedExcel) return false;

                Dictionary<string, string> headers = GetExcelHeaders(excelFile);

                string productIDCol = headers.GetValueOrDefault("produktid");
                string productNameCol = headers.GetValueOrDefault("produkt");
                string productGroupCol = headers.GetValueOrDefault("produktgrupp");
                string productCategoryCol = headers.GetValueOrDefault("produktkategori");
                string productDepartmentCol = headers.GetValueOrDefault("avdelning");

                if (productIDCol is null || productNameCol is null || productGroupCol is null ||
                    productCategoryCol is null || productDepartmentCol is null) return true;

                int currentRow = 1;
                while (true)
                {
                    string rowProductID = excelFile.ReadCell<string>(productIDCol, ++currentRow);
                    if (rowProductID is null) return true;

                    string rowProductGroupID = rowProductID.Substring(4, 2);
                    string rowProductName = excelFile.ReadCell<string>(productNameCol, currentRow);
                    string rowProductGroup = excelFile.ReadCell<string>(productGroupCol, currentRow);
                    string rowProductCategory = excelFile.ReadCell<string>(productCategoryCol, currentRow).ToLower();
                    string rowProductDepartment = excelFile.ReadCell<string>(productDepartmentCol, currentRow).ToLower();

                    if (rowProductID.Length == 0) return true;
                    if (rowProductID.Length != 6 || string.IsNullOrWhiteSpace(rowProductName) || string.IsNullOrWhiteSpace(rowProductGroup)) continue;

                    if (products.Contains(rowProductID)) continue;

                    Produktkategori productCategoryDB = possibleCategories.FirstOrDefault(pc => pc.Benämning.ToLower() == rowProductCategory);
                    if (productCategoryDB is null) continue;

                    Avdelning departmentDB = possibleDepartments.FirstOrDefault(d => d.Benämning.ToLower() == rowProductDepartment);
                    if (departmentDB is null) continue;

                    Produktgrupp productGroupDB = productGroups.FirstOrDefault(pg => pg.ProduktgruppID == rowProductGroupID);

                    if (productGroupDB is null)
                    {
                        productGroupDB = new Produktgrupp()
                        {
                            ProduktgruppID = rowProductGroupID,
                            Benämning = rowProductGroup,
                            Produktkategori = productCategoryDB
                        };

                        productGroups.Add(productGroupDB);
                    }

                    products.Add(rowProductID);

                    unitOfWork.ProductRepository.Add(new Produkt()
                    {
                        ProduktID = rowProductID,
                        Produktnamn = rowProductName,
                        Produktgrupp = productGroupDB,
                        Avdelning = departmentDB
                    });
                }
            }
        }

        public void ImportProducts()
        {
            Directory.CreateDirectory(productsDir);

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                List<string> products = unitOfWork.ProductRepository.FindAll().Select(p => p.ProduktID).ToList();
                List<Produktgrupp> productGroups = unitOfWork.ProductGroupRepository.FindAll().ToList();

                IEnumerable<Produktkategori> possibleCategories = unitOfWork.ProductCategoryRepository.FindAll();
                IEnumerable<Avdelning> possibleDepartments = unitOfWork.DepartmentRepository.FindAll(
                    d => d.Avdelningstyp.AvdelningstypID == "PROD",
                    d => d.Avdelningstyp);

                foreach (string productFile in Directory.EnumerateFiles(productsDir))
                {
                    if (!ImportProductFile(productFile, unitOfWork, products, productGroups, possibleCategories, possibleDepartments)) break;
                    FileUtils.MoveFileToDir(productFile, productsHandledDir);
                }
                unitOfWork.Save();
            }
        }

        private bool ImportCustomerFile(string customerFile, UnitOfWork unitOfWork, List<string> customers, IEnumerable<Kundkategori> possibleCustomerCategories)
        {
            using (ExcelFile excelFile = new ExcelFile(customerFile, true, false))
            {
                if (!excelFile.LoadedExcel) return false;

                Dictionary<string, string> headers = GetExcelHeaders(excelFile);

                string customerIDCol = headers.GetValueOrDefault("kundid");
                string customerNameCol = headers.GetValueOrDefault("kund");
                string customerCategoryCol = headers.GetValueOrDefault("kundkategori");

                if (customerIDCol is null || customerNameCol is null || customerCategoryCol is null) return true;

                int currentRow = 1;
                while (true)
                {
                    string rowCustomerID = excelFile.ReadCell<string>(customerIDCol, ++currentRow);
                    string rowCustomerName = excelFile.ReadCell<string>(customerNameCol, currentRow);
                    string rowCustomerCategory = excelFile.ReadCell<string>(customerCategoryCol, currentRow);

                    if (string.IsNullOrWhiteSpace(rowCustomerID)) return true;
                    if (rowCustomerID.Length != 4) continue;
                    if (string.IsNullOrWhiteSpace(rowCustomerName)) continue;

                    if (customers.Contains(rowCustomerID)) continue;

                    rowCustomerCategory = rowCustomerCategory.ToLower();

                    Kundkategori customerCategoryDB = possibleCustomerCategories.FirstOrDefault(cc => cc.Benämning.ToLower() == rowCustomerCategory);
                    if (customerCategoryDB is null) continue;

                    customers.Add(rowCustomerID);

                    unitOfWork.CustomerRepository.Add(new Kund()
                    {
                        KundID = rowCustomerID,
                        Kundnamn = rowCustomerName,
                        Kundkategori = customerCategoryDB
                    });
                }
            }
        }

        public void ImportCustomers()
        {
            Directory.CreateDirectory(customersDir);

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                List<string> customers = unitOfWork.CustomerRepository.FindAll().Select(c => c.KundID).ToList();

                IEnumerable<Kundkategori> possibleCustomerCategories = unitOfWork.CustomerCategoryRepository.FindAll();

                foreach (string customerFile in Directory.EnumerateFiles(customersDir))
                {
                    if (!ImportCustomerFile(customerFile, unitOfWork, customers, possibleCustomerCategories)) break;
                    FileUtils.MoveFileToDir(customerFile, customersHandledDir);
                }
                unitOfWork.Save();
            }
        }

        private bool ImportEmployeeFile(string employeeFile, UnitOfWork unitOfWork, List<string> employees, Regex ssnRegex, Avdelning admDept, Avdelning salesDept, Avdelning devManDept, Avdelning operationsDept)
        {
            using (ExcelFile excelFile = new ExcelFile(employeeFile, true, false))
            {
                if (!excelFile.LoadedExcel) return false;

                Dictionary<string, string> headers = GetExcelHeaders(excelFile);

                string ssnCol = headers.GetValueOrDefault("pnr");
                string nameCol = headers.GetValueOrDefault("namn");
                string salaryCol = headers.GetValueOrDefault("månadslön");
                string employmentRateCol = headers.GetValueOrDefault("sysselsättningsgrad");
                string vacancyDeductionCol = headers.GetValueOrDefault("vakansavdrag");
                string admCol = headers.GetValueOrDefault("adm");
                string salesCol = headers.GetValueOrDefault("förs/mark");
                string devManCol = headers.GetValueOrDefault("utvförv");
                string operationCol = headers.GetValueOrDefault("drift");

                if (ssnCol is null || nameCol is null || salaryCol is null || employmentRateCol is null ||
                    vacancyDeductionCol is null || admCol is null || salesCol is null || devManCol is null ||
                    operationCol is null) return true;

                int currentRow = 1;
                while (true)
                {
                    string ssnRow = excelFile.ReadCell<string>(ssnCol, ++currentRow);
                    string nameRow = excelFile.ReadCell<string>(nameCol, currentRow);
                    double salaryRow = excelFile.ReadCell<double>(salaryCol, currentRow);
                    double employmentRateRow = excelFile.ReadCell<double>(employmentRateCol, currentRow) * 100;
                    double vacancyDeductionRow = excelFile.ReadCell<double>(vacancyDeductionCol, currentRow) * 100;
                    double admRow = excelFile.ReadCell<double>(admCol, currentRow) * 100;
                    double salesRow = excelFile.ReadCell<double>(salesCol, currentRow) * 100;
                    double devManRow = excelFile.ReadCell<double>(devManCol, currentRow) * 100;
                    double operationRow = excelFile.ReadCell<double>(operationCol, currentRow) * 100;

                    if (string.IsNullOrWhiteSpace(ssnRow)) return true;
                    if (!ssnRegex.IsMatch(ssnRow)) continue;
                    if (string.IsNullOrWhiteSpace(nameRow)) continue;

                    if (employees.Contains(ssnRow)) continue;


                    List<Avdelningfördelning> departmentDistribution = new List<Avdelningfördelning>();
                    if (admRow > 0)
                        departmentDistribution.Add(new Avdelningfördelning()
                        {
                            Avdelning = admDept,
                            Andel = admRow
                        });
                    if (salesRow > 0)
                        departmentDistribution.Add(new Avdelningfördelning()
                        {
                            Avdelning = salesDept,
                            Andel = salesRow
                        });
                    if (devManRow > 0)
                        departmentDistribution.Add(new Avdelningfördelning()
                        {
                            Avdelning = devManDept,
                            Andel = devManRow
                        });
                    if (operationRow > 0)
                        departmentDistribution.Add(new Avdelningfördelning()
                        {
                            Avdelning = operationsDept,
                            Andel = operationRow
                        });

                    if (departmentDistribution.Sum(dd => dd.Andel) != employmentRateRow - vacancyDeductionRow) continue;

                    employees.Add(ssnRow);

                    unitOfWork.EmployeeRepository.Add(new Personal()
                    {
                        Personnummer = ssnRow,
                        Namn = nameRow,
                        Månadslön = salaryRow,
                        Vakansavdrag = vacancyDeductionRow,
                        Sysselsättningsgrad = employmentRateRow,
                        Avdelningfördelningar = departmentDistribution
                    });
                }
            }
        }

        public void ImportEmployees()
        {
            Directory.CreateDirectory(employeesDir);

            Regex ssnRegex = new Regex(@"^\d{6}\-\d{4}$");

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                List<string> employees = unitOfWork.EmployeeRepository.FindAll().Select(e => e.Personnummer).ToList();

                Avdelning admDept = unitOfWork.DepartmentRepository.Find(d => d.AvdelningID == "AO");
                Avdelning operationsDept = unitOfWork.DepartmentRepository.Find(d => d.AvdelningID == "DR");
                Avdelning salesDept = unitOfWork.DepartmentRepository.Find(d => d.AvdelningID == "FO");
                Avdelning devManDept = unitOfWork.DepartmentRepository.Find(d => d.AvdelningID == "UF");

                if (admDept is null || operationsDept is null || salesDept is null || devManDept is null) return;

                foreach (string employeeFile in Directory.EnumerateFiles(employeesDir))
                {
                    if (!ImportEmployeeFile(employeeFile, unitOfWork, employees, ssnRegex, admDept, salesDept, devManDept, operationsDept)) break;
                    FileUtils.MoveFileToDir(employeeFile, employeesHandledDir);
                }
                unitOfWork.Save();
            }
        }

        private bool ImportAccountFile(string customerFile, UnitOfWork unitOfWork, List<int> accounts)
        {
            using (ExcelFile excelFile = new ExcelFile(customerFile, true, false))
            {
                if (!excelFile.LoadedExcel) return false;

                Dictionary<string, string> headers = GetExcelHeaders(excelFile);

                string accountCol = headers.GetValueOrDefault("konto");
                string descriptionCol = headers.GetValueOrDefault("kontobenämning");

                if (accountCol is null || descriptionCol is null) return true;

                int currentRow = 1;
                while (true)
                {
                    string accountRowStr = excelFile.ReadCell<string>(accountCol, ++currentRow);
                    string descriptionRow = excelFile.ReadCell<string>(descriptionCol, currentRow);

                    int accountRow = 0;
                    int.TryParse(accountRowStr, out accountRow);

                    if (accountRow == 0) return true;
                    if (accountRow < 4000 || accountRow > 8999 || accountRow == 5021) continue;
                    if (string.IsNullOrWhiteSpace(descriptionRow)) continue;

                    if (accounts.Contains(accountRow)) continue;

                    accounts.Add(accountRow);

                    unitOfWork.AccountRepository.Add(new Konto()
                    {
                        KontoID = accountRow,
                        Benämning = descriptionRow
                    });
                }
            }
        }

        public void ImportAccounts()
        {
            Directory.CreateDirectory(accountsDir);

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                List<int> accounts = unitOfWork.AccountRepository.FindAll().Select(c => c.KontoID).ToList();

                foreach (string accountsFile in Directory.EnumerateFiles(accountsDir))
                {
                    if (!ImportAccountFile(accountsFile, unitOfWork, accounts)) break;
                    FileUtils.MoveFileToDir(accountsFile, accountsHandledDir);
                }
                unitOfWork.Save();
            }
        }

        public string ExportBudgetResult(string title, double revenue, double costs)
        {
            Directory.CreateDirectory(budgetResultDir);
            string datetimeStr = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"{title}_{datetimeStr}.xlsx";
            using (ExcelFile excelFile = new ExcelFile($@"{budgetResultDir}\{filename}", false, true))
            {
                if (!excelFile.LoadedExcel) return null;

                excelFile.WriteCell("A", 1, "Titel");
                excelFile.WriteCell("A", 2, "Intäkt");
                excelFile.WriteCell("A", 3, "Kostnad");
                excelFile.WriteCell("A", 4, "Resultat");

                excelFile.WriteCell("B", 1, title);
                excelFile.WriteCell("B", 2, revenue.ToString());
                excelFile.WriteCell("B", 3, costs.ToString());
                excelFile.WriteCell("B", 4, "=B2-B3");

                excelFile.Save();
            }

            return filename;
        }
    }
}
