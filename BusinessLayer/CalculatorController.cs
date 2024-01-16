using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class CalculatorController
    {
        private readonly UnitOfWork unitOfWork = new UnitOfWork();

        private readonly Dictionary<string, double> costProduct = new Dictionary<string, double>();
        private readonly string costProductDir = $@"{AppDomain.CurrentDomain.BaseDirectory}KostnadProdukt";

        public CalculatorController()
        {
            Directory.CreateDirectory(costProductDir);
            Regex fileRowRegex = new Regex("\t+");
            foreach (string costProductFile in Directory.GetFiles(costProductDir))
            {
                using (StreamReader fileStream = new StreamReader(costProductFile, System.Text.Encoding.UTF8))
                {
                    string line;
                    while ((line = fileStream.ReadLine()?.Trim()) is not null)
                    {
                        string[] lineParts = fileRowRegex.Split(line);
                        if (lineParts.Length != 4) continue;

                        string lineID = lineParts[0];
                        double lineAmount;

                        if (!double.TryParse(lineParts[3], out lineAmount)) continue;

                        if (costProduct.ContainsKey(lineID)) costProduct[lineID] += lineAmount;
                        else costProduct[lineID] = lineAmount;
                    }
                }
            }
        }

        public Tuple<double, double> CalculateProduct(Produkt product)
        {
            Avkastningskrav currentROI = unitOfWork.ROIRepository.Find(roi => true);
            double currentROIAmount = currentROI == null ? 0 : currentROI.Belopp;

            Produkt productDB = unitOfWork.ProductRepository.Find(p => p.ProduktID == product.ProduktID,
                p => p.Avdelning,
                p => p.Schablonkostnader,
                p => p.Intäktsbudgeteringar,
                p => p.Produktfördelningar, p => p.Produktfördelningar.Select(pd => pd.Avdelningfördelning), p => p.Produktfördelningar.Select(pd => pd.Avdelningfördelning.Personal)
            );

            double internalCost = GetInternalCosts();

            double totalProductDistributionsSum = unitOfWork.ProductDistributionRepository.FindAll().Sum(pd => pd.Andel);
            
            return DoProduct(productDB, currentROIAmount, internalCost, totalProductDistributionsSum);
        }

        public Tuple<double, double> CalculateProductGroup(Produktgrupp productgroup)
        {
            Avkastningskrav currentROI = unitOfWork.ROIRepository.Find(roi => true);
            double currentROIAmount = currentROI == null ? 0 : currentROI.Belopp;

            IEnumerable<Produkt> productsDB = unitOfWork.ProductRepository.FindAll(p => p.Produktgrupp.ProduktgruppID == productgroup.ProduktgruppID,
                p => p.Produktgrupp,
                p => p.Avdelning,
                p => p.Schablonkostnader,
                p => p.Intäktsbudgeteringar,
                p => p.Produktfördelningar, p => p.Produktfördelningar.Select(pd => pd.Avdelningfördelning), p => p.Produktfördelningar.Select(pd => pd.Avdelningfördelning.Personal)
            );

            double internalCost = GetInternalCosts();

            double totalProductDistributionsSum = unitOfWork.ProductDistributionRepository.FindAll().Sum(pd => pd.Andel);

            double cost = 0d;
            double revenue = 0d;

            foreach (Produkt product in productsDB)
            {
                Tuple<double, double> productData = DoProduct(product, currentROIAmount, internalCost, totalProductDistributionsSum);
                revenue += productData.Item1;
                cost += productData.Item2;
            }

            return new Tuple<double, double>(revenue, cost);
        }

        public Tuple<double, double> CalculateDepartment(Avdelning department)
        {
            Avkastningskrav currentROI = unitOfWork.ROIRepository.Find(roi => true);
            double currentROIAmount = currentROI == null ? 0 : currentROI.Belopp;

            IEnumerable<Produkt> productsDB = unitOfWork.ProductRepository.FindAll(p => p.Avdelning.AvdelningID == department.AvdelningID,
                p => p.Avdelning,
                p => p.Schablonkostnader,
                p => p.Intäktsbudgeteringar,
                p => p.Produktfördelningar, p => p.Produktfördelningar.Select(pd => pd.Avdelningfördelning), p => p.Produktfördelningar.Select(pd => pd.Avdelningfördelning.Personal)
            );

            double internalCost = GetInternalCosts();

            double totalProductDistributionsSum = unitOfWork.ProductDistributionRepository.FindAll().Sum(pd => pd.Andel);

            double cost = 0d;
            double revenue = 0d;

            foreach (Produkt product in productsDB)
            {
                Tuple<double, double> productData = DoProduct(product, currentROIAmount, internalCost, totalProductDistributionsSum);
                revenue += productData.Item1;
                cost += productData.Item2;
            }
            
            return new Tuple<double, double>(revenue, cost);
        }

        public Tuple<double, double> CalculateOrganisation()
        {
            Avkastningskrav currentROI = unitOfWork.ROIRepository.Find(roi => true);
            double currentROIAmount = currentROI == null ? 0 : currentROI.Belopp;

            IEnumerable<Produkt> productsDB = unitOfWork.ProductRepository.FindAll(
                p => p.Avdelning,
                p => p.Schablonkostnader,
                p => p.Intäktsbudgeteringar,
                p => p.Produktfördelningar, p => p.Produktfördelningar.Select(pd => pd.Avdelningfördelning), p => p.Produktfördelningar.Select(pd => pd.Avdelningfördelning.Personal)
            );

            double internalCost = GetInternalCosts();

            double totalProductDistributionsSum = unitOfWork.ProductDistributionRepository.FindAll().Sum(pd => pd.Andel);

            double cost = 0d;
            double revenue = 0d;

            foreach (Produkt product in productsDB)
            {
                Tuple<double, double> productData = DoProduct(product, currentROIAmount, internalCost, totalProductDistributionsSum);
                revenue += productData.Item1;
                cost += productData.Item2;
            }

            return new Tuple<double, double>(revenue, cost);
        }

        private Tuple<double, double> DoProduct(Produkt product, double currentROIAmount, double internalCost, double totalProductDistributionsSum)
        {
            double cost = product.Schablonkostnader.Sum(s => s.Belopp);
            double revenue = product.Intäktsbudgeteringar.Sum(rb => rb.Budget);

            double productDistributionSum = product.Produktfördelningar.Sum(pd => pd.Andel);
            double productDistributionShare;
            if (totalProductDistributionsSum == 0d) productDistributionShare = 0;
            else productDistributionShare = productDistributionSum / totalProductDistributionsSum;

            revenue += currentROIAmount * productDistributionShare;
            cost += internalCost * productDistributionShare;

            cost += product.Produktfördelningar.Sum(pd => pd.Avdelningfördelning.Personal.Månadslön * (pd.Andel / 100d));

            if (costProduct.ContainsKey(product.ProduktID)) cost += costProduct[product.ProduktID];

            return new Tuple<double, double>(revenue, cost);
        }

        private double GetInternalCosts()
        {
            double sum = unitOfWork.SchablonExpenseActivitiesRepository.FindAll().Sum(s => s.Belopp);
            sum += unitOfWork.SchablonExpenseOfficeRepository.FindAll().Sum(s => s.Belopp);
            sum += unitOfWork.DepartmentDistributionRepository.FindAll(dd => dd.Avdelning.Avdelningstyp.AvdelningstypID == "AFFO",
                dd => dd.Avdelning, dd => dd.Avdelning.Avdelningstyp, dd => dd.Personal).Sum(dd => dd.Personal.Månadslön * dd.Andel);
            return sum;
        }
    }
}
