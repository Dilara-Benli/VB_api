using VB_api.Models;
using Microsoft.EntityFrameworkCore;

namespace VB_api.Data 
{
    public class BankDbContext : DbContext
    {
        public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

        // tabloları ekle    
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<TransactionType> TransactionType { get; set; }
        public DbSet<Transfer> Transfer { get; set; }
    }
}