using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentValidationApp.Models
{
    public class AppDbContext : DbContext
    {
        /* constructor DbContextOptions isminde generic bir sınıf alacak. Bu generic AppDbContext yapımız olacak.
         * parametre ismine options diyelim. options parametresini ana sınıfımız DbContext'in constructor'ına göndereceğiz.
         * options'ı startup.cs de belirtmemiz gerekir. sqlserver kullanacağını */
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
    }
}
