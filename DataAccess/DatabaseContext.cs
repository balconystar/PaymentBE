using Microsoft.EntityFrameworkCore;
using PaymentBE.Models;

namespace PaymentBE.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<Money> Money { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<OTP> OTP { get; set; }
        public DbSet<Deposits> Deposits { get; set; }
        public DbSet<Withdraws> Withdraws { get; set; }
        public DbSet<Transfers> Transfers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure that MoneyToken is unique
            modelBuilder.Entity<Money>()
                .HasIndex(m => m.MoneyToken)
                .IsUnique();  // Enforce uniqueness at the database level
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
