using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class ProductController
    {
        public IEnumerable<Avdelning> GetAllProductDepartments()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.DepartmentRepository.FindAll(d => d.Avdelningstyp.Benämning == "Produktion", p => p.Avdelningstyp);
            }
        }

        public IEnumerable<Produktkategori> GetAllProductCategories()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ProductCategoryRepository.FindAll();
            }
        }

        public IEnumerable<Produktgrupp> GetAllProductGroups()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ProductGroupRepository.FindAll(p => p.Produktkategori);
            }
        }

        public IEnumerable<Produkt> GetAllProducts()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ProductRepository.FindAll(p => p.Avdelning,
                    p => p.Produktgrupp, p => p.Produktgrupp.Produktkategori).ToList();
            }
        }

        public IEnumerable<Produkt> GetAllProductsByCategory(Produktgrupp productGroup)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ProductRepository.FindAll(p => p.Produktgrupp.ProduktgruppID == productGroup.ProduktgruppID,
                    p => p.Avdelning, p => p.Produktgrupp);
            }
        }

        public IEnumerable<Produkt> GetAllProductsByDepartment(Avdelning department)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ProductRepository.FindAll(p => p.Avdelning.AvdelningID == department.AvdelningID,
                    p => p.Avdelning);
            }
        }

        public IEnumerable<Produktfördelning> GetAllProductDistributionsByProduct(Produkt product)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ProductDistributionRepository.FindAll(
                    pd => pd.Produkt.ProduktID == product.ProduktID,
                    p => p.Produkt, p => p.Avdelningfördelning, p => p.Avdelningfördelning.Personal);
            }
        }

        public Produktgrupp CreateProductGroup(string productGroupID, string description, Produktkategori productCategory)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Produktkategori productCategoryDB = unitOfWork.ProductCategoryRepository.Find(pc => pc.ProduktkategoriID == productCategory.ProduktkategoriID);

                if (productCategoryDB is null) return null;
                
                Produktgrupp productGroupDB = unitOfWork.ProductGroupRepository.Find(pc => pc.ProduktgruppID == productGroupID);
                if (!(productGroupDB is null)) return null;

                Produktgrupp newProductGroup = new Produktgrupp()
                {
                    ProduktgruppID = productGroupID,
                    Benämning = description,
                    Produktkategori = productCategoryDB
                };

                unitOfWork.ProductGroupRepository.Add(newProductGroup);
                unitOfWork.Save();
                return newProductGroup;
            }
        }

        public Produktgrupp UpdateProductGroup(Produktgrupp editedProductGroup, string description, Produktkategori productCategory)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Produktkategori productCategoryDB = unitOfWork.ProductCategoryRepository.Find(pc => pc.ProduktkategoriID == productCategory.ProduktkategoriID);

                if (productCategoryDB is null) return null;

                Produktgrupp productGroupDB = unitOfWork.ProductGroupRepository.Find(pc => pc.ProduktgruppID == editedProductGroup.ProduktgruppID);
                if (!(productGroupDB is not null)) return null;

                productGroupDB.Benämning = description;
                productGroupDB.Produktkategori = productCategoryDB;
                unitOfWork.Save();

                return productGroupDB;
            }
        }

        public void DeleteProductGroup(Produktgrupp productGroup)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Produktgrupp productGroupDB = unitOfWork.ProductGroupRepository.Find(pg => pg.ProduktgruppID == productGroup.ProduktgruppID);
                if (productGroupDB is null) return;

                unitOfWork.ProductGroupRepository.Remove(productGroupDB);
                unitOfWork.Save();
            }
        }

        public Produkt CreateProduct(string productID, string productName, Produktgrupp productGroup, Avdelning department)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Produktgrupp productGroupDB = unitOfWork.ProductGroupRepository.Find(pg => pg.ProduktgruppID == productGroup.ProduktgruppID);
                if (productGroupDB is null) return null;

                Avdelning departmentDB = unitOfWork.DepartmentRepository.Find(d => d.AvdelningID == department.AvdelningID);
                if (departmentDB is null) return null;

                string actualProductID = productID + productGroupDB.ProduktgruppID;

                Produkt productDB = unitOfWork.ProductRepository.Find(p => p.ProduktID == actualProductID);
                if (productDB is not null) return null;

                Produkt newProduct = new Produkt()
                {
                    ProduktID = actualProductID,
                    Produktnamn = productName,
                    Produktgrupp = productGroupDB,
                    Avdelning = departmentDB
                };

                unitOfWork.ProductRepository.Add(newProduct);
                unitOfWork.Save();

                return newProduct;
            }
        }

        public void DeleteProduct(Produkt product)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Produkt productDB = unitOfWork.ProductRepository.Find(p => p.ProduktID == product.ProduktID);
                if (productDB is null) return;

                unitOfWork.ProductRepository.Remove(productDB);
                unitOfWork.Save();
            }
        }

        public Produkt UpdateProduct(Produkt product, string productName, Avdelning department)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Avdelning departmentDB = unitOfWork.DepartmentRepository.Find(d => d.AvdelningID == department.AvdelningID);
                if (departmentDB is null) return null;

                Produkt productDB = unitOfWork.ProductRepository.Find(p => p.ProduktID == product.ProduktID, p => p.Produktgrupp, p => p.Produktgrupp.Produktkategori);
                if (productDB is null) return null;

                productDB.Produktnamn = productName;
                productDB.Avdelning = departmentDB;

                unitOfWork.Save();

                return productDB;
            }
        }

        public IEnumerable<Produktfördelning> GetAllProductDistributionsByDepartmentEmpty(Avdelning department)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ProductRepository.FindAll(a => a.Avdelning.AvdelningID == department.AvdelningID, p => p.Avdelning).Select(p => new Produktfördelning() { Produkt = p });
            }
        }

        public IEnumerable<Produktfördelning> GetAllProductDistributions()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.ProductDistributionRepository.FindAll(p => p.Produkt);
            }
        }

        public void LockDepartmentProductExpenses(Avdelning department)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Avdelning departmentDB = unitOfWork.DepartmentRepository.Find(d => d.AvdelningID == department.AvdelningID);
                if (departmentDB is null) return;
                departmentDB.KostnadsbudgetProduktLåst = true;
                unitOfWork.Save();
            }
        }
    }
}
