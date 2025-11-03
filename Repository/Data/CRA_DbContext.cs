using Microsoft.EntityFrameworkCore;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data
{
    public class CRA_DbContext : DbContext
    {
        public CRA_DbContext(DbContextOptions options) : base(options)
        {
        }

        public CRA_DbContext()
        {
        }

        //New Table here
        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Schedules> Schedules { get; set; }
        public DbSet<PersistNotif> PersistNotifs { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Booking> BookingHistories { get; set; }
        public DbSet<CarRegistration> CarRegistrations { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }
        public DbSet<ParkingLot> ParkingLots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seeding Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1001, Title = "Admin" },
                new Role { Id = 1002, Title = "Staff" },
                new Role { Id = 1, Title = "Customer" }
            );

            //New relationships here
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(u => u.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Car>()
                .HasOne(u => u.Owner)
                .WithMany(u => u.Cars)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Car>()
                .HasOne(u => u.PreferredLot)
                .WithMany(u => u.Cars)
                .HasForeignKey(u => u.PrefLotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(u => u.InvoicesAsCustomer)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Vendor)
                .WithMany(u => u.InvoicesAsVendor)
                .HasForeignKey(i => i.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.PartyA)
                .WithMany() //.WithMany(u => u.ContractsAsPartyA) 
                .HasForeignKey(c => c.PartyAId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.PartyB)
                .WithMany() //.WithMany(u => u.ContractsAsPartyB)
                .HasForeignKey(c => c.PartyBId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Invoice)
                .WithMany() //assuming one contract per invoice
                .HasForeignKey(c => c.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}