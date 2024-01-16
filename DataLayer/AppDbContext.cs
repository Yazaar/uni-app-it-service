using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class AppDbContext : DbContext
    {
        private static string dbConnectionStringFile = $"{AppDomain.CurrentDomain.BaseDirectory}db.txt";

        public DbSet<Roll> Roll { get; set; }
        public DbSet<Produkt> Produkt { get; set; }
        public DbSet<Personal> Personal { get; set; }
        public DbSet<Låsning> Låsning { get; set; }
        public DbSet<Avkastningskrav> Avkastningskrav { get; set; }

        public string ConnectionString { get; }
        public AppDbContext()
        {
            ConnectionString = GetConnectionStringFromFile();
            if (ConnectionString is null) ConnectionString = Environment.GetEnvironmentVariable("it-service-db");
            if (ConnectionString is null) ConnectionString = "Data Source=(localdb\\MSSQLLocalDB;Initial Catalog=itservice;Integrated Security=True";
        }

        private string GetConnectionStringFromFile()
        {
            if (!File.Exists(dbConnectionStringFile))
            {
                File.Create(dbConnectionStringFile);
                return null;
            }

            using (StreamReader fileStream = new StreamReader(dbConnectionStringFile))
            {
                string cs = fileStream.ReadToEnd();

                if (string.IsNullOrWhiteSpace(cs)) return null;
                else return cs;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
}
