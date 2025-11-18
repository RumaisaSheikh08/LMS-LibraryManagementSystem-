using System;
using LMSAvanza.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Identity.Client;

namespace LMSAvanza.Data
{
    public class ApplicationDBContext: DbContext
    {
        public DbSet<StudentModel> Students { get; set; }
        public DbSet<AdminModel> Admin {  get; set; }
        public DbSet<BooksModel> Books {  get; set; }
        public DbSet<TransactionModel> Transactions {  get; set; }
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentModel>()
                .HasKey(s => s.StudentID);

            modelBuilder.Entity<AdminModel>()
                .HasKey(a => a.AdminID);

            modelBuilder.Entity<BooksModel>()
                .HasKey(b => b.BookID);

            modelBuilder.Entity<TransactionModel>()
                .HasKey(t => t.TxnID);

        }
    }
}
