using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; }
        //Only nullable for google login, verify in services instead of model
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? ImageAvatar { get; set; }
        public bool IsGoogle { get; set; }
        public string? GoogleId { get; set; }

        public bool IsCarOwner { get; set; }
        public double Rating { get; set; }

        public string Status { get; set; }

        public int RoleId { get; set; }
        public int Gender { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        public virtual ICollection<Car>? Cars { get; set; } = new List<Car>();
        public virtual ICollection<Invoice> InvoicesAsCustomer { get; set; } = new List<Invoice>();
        public virtual ICollection<Invoice> InvoicesAsVendor { get; set; } = new List<Invoice>();
        public virtual ICollection<Booking>? BookingHistory { get; set; } = new List<Booking>();
        public virtual ICollection<RefreshToken>? RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
