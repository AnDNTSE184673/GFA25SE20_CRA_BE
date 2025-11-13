using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO
{
    public class RegisterOwnerRequest
    {
        public string Username { get; set; }
        //Only nullable for google login, verify in services instead of model
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? ImageAvatar { get; set; }
        public bool IsCarOwner { get; set; } = true;
        public double Rating { get; set; }
        public string Status { get; set; } = "Active";
        public int RoleId { get; set; }
        public int Gender { get; set; } = 1;
    }
}
