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
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Feedback> Feedback { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seeding Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1001, Title = "Admin" },
                new Role { Id = 1002, Title = "Staff" },
                new Role { Id = 1, Title = "Customer" }
            );

            // Seeding Genders
            modelBuilder.Entity<Gender>().HasData(
                new Gender { Id = 1, GenderTitle = "Male" },
                new Gender { Id = 2, GenderTitle = "Female" },
                new Gender { Id = 3, GenderTitle = "Other" }
            );

            // Seeding Statuses
            modelBuilder.Entity<Status>().HasData(
                new Status { Id = 1, StatusName = "Active" },
                new Status { Id = 2, StatusName = "Inactive" },
                new Status { Id = 3, StatusName = "Pending" }
            );

            //New relationships here
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(u => u.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Gender)
                .WithMany()
                .HasForeignKey(u => u.GenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Status)
                .WithMany()
                .HasForeignKey(u => u.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Car>()
                .HasOne(u => u.Owner)
                .WithMany(u => u.Cars)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
