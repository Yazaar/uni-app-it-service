using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class CustomerController
    {
        public Kund CreateCustomer(string customerID, string customerName, Kundkategori customerCategory)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Kundkategori customerCategoryDB = unitOfWork.CustomerCategoryRepository.Find(cc => cc.KundkategoriID == customerCategory.KundkategoriID);
                if (customerCategoryDB is null) return null;

                Kund customerDB = unitOfWork.CustomerRepository.Find(c => c.KundID == customerID);
                if (customerDB is not null) return null;

                Kund newCustomer = new Kund()
                {
                    KundID = customerID,
                    Kundnamn = customerName,
                    Kundkategori = customerCategoryDB
                };

                unitOfWork.CustomerRepository.Add(newCustomer);
                unitOfWork.Save();
                return newCustomer;
            }
        }

        public void DeleteCustomer(Kund customer)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Kund customerDB = unitOfWork.CustomerRepository.Find(c => c.KundID == customer.KundID);
                if (customerDB is null) return;

                unitOfWork.CustomerRepository.Remove(customerDB);
                unitOfWork.Save();
            }
        }

        public ICollection<Kundkategori> GetAllCustomerCategories()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.CustomerCategoryRepository.FindAll().ToList();
            }
        }

        public IEnumerable<Kund> GetAllCustomers()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.CustomerRepository.FindAll(c => c.Kundkategori).ToList();
            }
        }

        public Kund UpdateCustomer(Kund customer, string customerName, Kundkategori customerCategory)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Kundkategori customerCategoryDB = unitOfWork.CustomerCategoryRepository.Find(cc => cc.KundkategoriID == customerCategory.KundkategoriID);
                if (customerCategoryDB is null) return null;

                Kund customerDB = unitOfWork.CustomerRepository.Find(c => c.KundID == customer.KundID);
                if (customerDB is null) return null;

                customerDB.Kundnamn = customerName;
                customerDB.Kundkategori = customerCategoryDB;

                unitOfWork.Save();
                return customerDB;
            }
        }

        public IEnumerable<Intäktsbudgetering> GetAllRevenueBudgetsByCustomer(Kund customer)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.RevenueBudgetsRepository.FindAll(rb => rb.Kund.KundID == customer.KundID, c => c.Kund, c => c.Produkt);
            }
        }
    }
}
