using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.User
{
    public class UserView
    {
        public string Username { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? ImageAvatar { get; set; }
        public bool IsGoogle { get; set; }

        public bool IsCarOwner { get; set; }
        public double Rating { get; set; }

        public string Status { get; set; }

        public int RoleId { get; set; }
        public int Gender { get; set; }
    }
}
