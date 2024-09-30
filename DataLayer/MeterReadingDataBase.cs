using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace DataLayer
{
    public class MeterReadingDataBase : DbContext
    {
        public MeterReadingDataBase(DbContextOptions<MeterReadingDataBase> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasKey(a => a.AccountId);
            modelBuilder.Entity<MeterReading>().HasKey(m => m.MeterReadingId);

            modelBuilder.Entity<MeterReading>()
                .HasOne(m => m.Account)
                .WithMany()
                .HasForeignKey(m => m.AccountId);
        }
    }
}

