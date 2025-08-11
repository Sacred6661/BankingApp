using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TransactionService.Data.Models;

namespace TransactionService.Data
{
    public class TransactionsDbContext : DbContext
    {
        public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options)
        : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<TransactionStatusEntity> TransactionStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionType>().HasData(
                new TransactionType { TransactionTypeId = 1, Name = "Deposit" },
                new TransactionType { TransactionTypeId = 2, Name = "Withdraw" },
                new TransactionType { TransactionTypeId = 3, Name = "Transfer" }
            );

            modelBuilder.Entity<TransactionStatusEntity>().HasData(
                new TransactionStatusEntity { TransactionStatusId = 1, Name = "Pending" },
                new TransactionStatusEntity { TransactionStatusId = 2, Name = "Accepted" },
                new TransactionStatusEntity { TransactionStatusId = 3, Name = "Rejected" }
            );
        }
    }
}
