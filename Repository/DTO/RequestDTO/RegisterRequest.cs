using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, Phone]
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public int Gender { get; set; } = 0;
    }
}
