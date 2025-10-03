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
        [Required]
        public string Username { get; set; }
        //Only nullable for google login, verify in services instead of model
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? ImageAvatar { get; set; }
        [Required]
        public bool IsGoogle { get; set; }
        public string? GoogleId { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public bool IsCarOwner { get; set; }
        public double Rating { get; set; }

        [Required]
        public int RoleId { get; set; }
        [Required]
        public int GenderId { get; set; }
        [Required]
        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; }
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        [ForeignKey("GenderId")]
        public virtual Gender Gender { get; set; }

        public virtual ICollection<Car>? Cars { get; set; } = new List<Car>();
    }
}
